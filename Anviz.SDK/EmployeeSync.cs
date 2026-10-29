using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    /// <summary>
    /// What a device must hold for an employee: its data and its fingerprint templates
    /// </summary>
    public class EmployeeData
    {
        public UserInfo User { get; }
        public Dictionary<Finger, byte[]> Fingerprints { get; } = new Dictionary<Finger, byte[]>();

        public EmployeeData(UserInfo user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }
    }

    public enum SyncStep
    {
        ReadingEmployees,
        DeletingEmployees,
        WritingEmployees,
        WritingFingerprints,
        Done
    }

    public class SyncProgress
    {
        public SyncStep Step { get; }
        public int Done { get; }
        public int Total { get; }

        internal SyncProgress(SyncStep step, int done, int total)
        {
            Step = step;
            Done = done;
            Total = total;
        }
    }

    public class SyncResult
    {
        /// <summary>
        /// Employees the device did not have
        /// </summary>
        public int Added { get; internal set; }
        /// <summary>
        /// Employees whose data (name, card, password, ...) changed
        /// </summary>
        public int Updated { get; internal set; }
        /// <summary>
        /// Employees deleted and written again, because the device had fingerprints of them that
        /// are not wanted anymore: the protocol cannot delete those one by one
        /// </summary>
        public int Recreated { get; internal set; }
        /// <summary>
        /// Employees of the device that are not wanted
        /// </summary>
        public int Deleted { get; internal set; }
        /// <summary>
        /// Employees whose data was already right (their fingerprints are written anyway)
        /// </summary>
        public int Unchanged { get; internal set; }
        public int FingerprintsWritten { get; internal set; }
        /// <summary>
        /// Why the synchronization of this device stopped, when it did; null when it completed
        /// </summary>
        public Exception Error { get; internal set; }

        public override string ToString()
        {
            return $"Added: {Added}, Updated: {Updated}, Recreated: {Recreated}, Deleted: {Deleted}, Unchanged: {Unchanged}, Fingerprints: {FingerprintsWritten}"
                + (Error == null ? "" : $", Error: {Error.Message}");
        }
    }

    public partial class AnvizDevice
    {
        /// <summary>
        /// Makes the device hold exactly these employees and fingerprints: deletes the employees it
        /// should not have, writes the ones that are missing or changed, and writes every
        /// fingerprint template (reading one to compare takes as long as writing it).
        /// Employees whose data is already right are not sent again.
        /// </summary>
        public async Task<SyncResult> SyncEmployees(IEnumerable<EmployeeData> employees,
            IProgress<SyncProgress> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var wanted = new Dictionary<ulong, EmployeeData>();
            foreach (var employee in employees)
            {
                if (wanted.ContainsKey(employee.User.Id))
                {
                    throw new ArgumentException($"Employee {employee.User.Id} is in the list twice", nameof(employees));
                }
                wanted.Add(employee.User.Id, employee);
            }
            var result = new SyncResult();

            progress?.Report(new SyncProgress(SyncStep.ReadingEmployees, 0, 1));
            var current = await GetEmployeesData();
            cancellationToken.ThrowIfCancellationRequested();

            var toDelete = new List<ulong>();
            var toWrite = new List<UserInfo>();
            foreach (var user in current)
            {
                if (!wanted.TryGetValue(user.Id, out var want))
                {
                    toDelete.Add(user.Id);
                    result.Deleted++;
                }
                else if (user.EnrolledFingerprints.Except(want.Fingerprints.Keys).Any())
                {
                    toDelete.Add(user.Id);
                    toWrite.Add(want.User);
                    result.Recreated++;
                }
                else if (!SameData(user, want.User))
                {
                    toWrite.Add(want.User);
                    result.Updated++;
                }
                else
                {
                    result.Unchanged++;
                }
            }
            var known = new HashSet<ulong>(current.Select(u => u.Id));
            foreach (var want in wanted.Values.Where(e => !known.Contains(e.User.Id)))
            {
                toWrite.Add(want.User);
                result.Added++;
            }

            if (toDelete.Count > 0 && toDelete.Count == current.Count)
            {
                // nothing of the device stays: one command for all
                progress?.Report(new SyncProgress(SyncStep.DeletingEmployees, 0, 1));
                await DeleteEmployeesData();
                progress?.Report(new SyncProgress(SyncStep.DeletingEmployees, 1, 1));
            }
            else
            {
                for (var i = 0; i < toDelete.Count; i++)
                {
                    progress?.Report(new SyncProgress(SyncStep.DeletingEmployees, i, toDelete.Count));
                    cancellationToken.ThrowIfCancellationRequested();
                    await DeleteEmployeesData(toDelete[i]);
                }
            }

            for (var i = 0; i < toWrite.Count; i += UserInfo.MAX_RECORDS)
            {
                progress?.Report(new SyncProgress(SyncStep.WritingEmployees, i, toWrite.Count));
                cancellationToken.ThrowIfCancellationRequested();
                await SetEmployeesData(toWrite.Skip(i).Take(UserInfo.MAX_RECORDS).ToList());
            }

            var templates = wanted.Values
                .SelectMany(e => e.Fingerprints.Select(f => new { e.User.Id, Finger = f.Key, Template = f.Value }))
                .ToList();
            foreach (var template in templates)
            {
                progress?.Report(new SyncProgress(SyncStep.WritingFingerprints, result.FingerprintsWritten, templates.Count));
                cancellationToken.ThrowIfCancellationRequested();
                await SetFingerprintTemplate(template.Id, template.Finger, template.Template);
                result.FingerprintsWritten++;
            }

            progress?.Report(new SyncProgress(SyncStep.Done, 1, 1));
            return result;
        }

        /// <summary>
        /// Whether the device holds the data of the employee. Not byte by byte: a W1 and a W2PRO
        /// read back the high bits of the password (PWDH8) and the reserved byte (Keep) as
        /// anything, whatever was written, and the fingerprint state is not written
        /// </summary>
        private static bool SameData(UserInfo device, UserInfo wanted)
        {
            return device.Id == wanted.Id
                && device.Password == wanted.Password
                && device.Card == wanted.Card
                && device.Name == wanted.Name
                && device.Department == wanted.Department
                && device.Group == wanted.Group
                && device.Mode == wanted.Mode
                && device.Message == wanted.Message;
        }
    }

    public static class AnvizSync
    {
        /// <summary>
        /// Synchronizes the employees of many devices at the same time (see
        /// <see cref="AnvizDevice.SyncEmployees"/>), at most maxParallel at once (0: all). A device
        /// that fails does not stop the others: its result has the <see cref="SyncResult.Error"/>
        /// </summary>
        public static async Task<Dictionary<AnvizDevice, SyncResult>> SyncEmployees(IEnumerable<AnvizDevice> devices,
            IEnumerable<EmployeeData> employees, int maxParallel = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = employees.ToList();
            var targets = devices.Distinct().ToList();
            var results = targets.ToDictionary(d => d, d => (SyncResult)null);
            using (var slots = new SemaphoreSlim(maxParallel > 0 ? maxParallel : Math.Max(1, targets.Count)))
            {
                await Task.WhenAll(targets.Select(async device =>
                {
                    await slots.WaitAsync(cancellationToken);
                    SyncResult result;
                    try
                    {
                        result = await device.SyncEmployees(list, null, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        result = new SyncResult { Error = ex };
                    }
                    finally
                    {
                        slots.Release();
                    }
                    lock (results)
                    {
                        results[device] = result;
                    }
                }));
            }
            return results;
        }
    }
}
