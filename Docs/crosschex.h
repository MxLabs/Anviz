#ifndef _CROSS_CHEX_H_
#define _CROSS_CHEX_H_

#include "myplatform.h"

#ifdef API_STATICLIB
#define API_EXTERN
#elif defined(WIN32) || defined(_WIN32)
#if defined(_PLAY_IMPL_EXPORT_)
#define API_EXTERN __declspec(dllexport)
#else
#define API_EXTERN __declspec(dllimport)
#endif
#else
#define API_EXTERN
#endif

#ifdef __cplusplus



extern "C" {
#endif

#define MAX_EMPLOYEE_ID             5
#define MAX_EMPLOYEE_NAME_ASCII     10
#define MAX_EMPLOYEE_NAME_UNICODE   20
#define MAX_EMPLOYEE_NAME_22        64
#define MAX_EMPLOYEE_NAME_DR        160
#define MAX_PWD_LEN                 3
#define MAC_CARD_ID_SMALL_LEN       3
#define MAC_CARD_ID_LEN             4
#define MAX_FRC_DR                  13
#define MAX_CURP_DR                 18
#define FP_STATUS_LEN               2
#define FINGERPRINT_DATA_LEN_15360  15360
#define FINGERPRINT_DATA_LEN_10240  10240
#define FINGERPRINT_DATA_LEN_6144   6144 // normal:338,OA1000PM:2048*3,OA1000P:2048
#define FINGERPRINT_DATA_LEN_2048   2048
#define FINGERPRINT_DATA_LEN_1200   1200
#define FINGERPRINT_DATA_LEN_338    338
#define SOFTWARE_VERSION_LEN        8
#define DEV_TYPE_LEN                8
#define ADDR_LEN                    24
#define DEV_TYPE_NUMBER             10
#define DEV_SERIAL_NUMBER           16


enum
{
    CCHEX_RET_RECORD_INFO_TYPE                  = 1,
    CCHEX_RET_DEV_LOGIN_TYPE                    = 2,
    CCHEX_RET_DEV_LOGOUT_TYPE                   = 3,
    CCHEX_RET_DLFINGERPRT_TYPE                  = 4,
    CCHEX_RET_ULFINGERPRT_TYPE                  = 5,

    CCHEX_RET_ULEMPLOYEE_INFO_TYPE              = 6,
    CCHEX_RET_ULEMPLOYEE2_INFO_TYPE             = 7,
    CCHEX_RET_ULEMPLOYEE2UNICODE_INFO_TYPE      = 8,

    CCHEX_RET_DLEMPLOYEE_INFO_TYPE              = 9,
    CCHEX_RET_DLEMPLOYEE2_INFO_TYPE             = 10,
    CCHEX_RET_DLEMPLOYEE2UNICODE_INFO_TYPE      = 11,

    CCHEX_RET_MSGGETBYIDX_INFO_TYPE             = 12,
    CCHEX_RET_MSGGETBYIDX_UNICODE_INFO_TYPE     = 13,
    CCHEX_RET_MSGADDNEW_INFO_TYPE               = 14,
    CCHEX_RET_MSGADDNEW_UNICODE_INFO_TYPE       = 15,
    CCHEX_RET_MSGDELBYIDX_INFO_TYPE             = 16,
    CCHEX_RET_MSGGETALLHEAD_INFO_TYPE           = 17,

    CCHEX_RET_REBOOT_TYPE                       = 18,
    CCHEX_RET_DEV_STATUS_TYPE                   = 19,
    CCHEX_RET_MSGGETALLHEADUNICODE_INFO_TYPE    = 20,
    CCHEX_RET_SETTIME_TYPE                      = 21,
    CCHEX_RET_UPLOADFILE_TYPE                   = 22,
    CCHEX_RET_GETNETCFG_TYPE                    = 23,
    CCHEX_RET_SETNETCFG_TYPE                    = 24,
    CCHEX_RET_GET_SN_TYPE                       = 25,
    CCHEX_RET_SET_SN_TYPE                       = 26,
    CCHEX_RET_DLEMPLOYEE_3_TYPE                 = 27, // 761
    CCHEX_RET_ULEMPLOYEE_3_TYPE                 = 28, // 761
    CCHEX_RET_GET_BASIC_CFG_TYPE                = 29,
    CCHEX_RET_SET_BASIC_CFG_TYPE                = 30,
    CCHEX_RET_DEL_EMPLOYEE_INFO_TYPE            = 31,
    CCHEX_RET_DLEMPLOYEE2UNICODE_DR_INFO_TYPE   = 32,
    CCHEX_RET_DEL_RECORD_OR_FLAG_INFO_TYPE      = 33,
    CCHEX_RET_MSGGETBYIDX_UNICODE_S_DATE_INFO_TYPE  = 34,       //for Seats
    CCHEX_RET_MSGADDNEW_UNICODE_S_DATE_INFO_TYPE    = 35,         //for Seats
    CCHEX_RET_MSGGETALLHEADUNICODE_S_DATE_INFO_TYPE = 36,       //for Seats

    CCHEX_RET_GET_BASIC_CFG2_TYPE               = 37,
    CCHEX_RET_SET_BASIC_CFG2_TYPE               = 38,
    CCHEX_RET_GETTIME_TYPE                      = 39,
    CCHEX_RET_INIT_USER_AREA_TYPE               = 40,
    CCHEX_RET_INIT_SYSTEM_TYPE                  = 41,
    CCHEX_RET_GET_PERIOD_TIME_TYPE              = 42,
    CCHEX_RET_SET_PERIOD_TIME_TYPE              = 43,
    CCHEX_RET_GET_TEAM_INFO_TYPE                = 44,
    CCHEX_RET_SET_TEAM_INFO_TYPE                = 45,
    CCHEX_RET_ADD_FINGERPRINT_ONLINE_TYPE       = 46,
    CCHEX_RET_FORCED_UNLOCK_TYPE                = 47,
    CCHEX_RET_UDP_SEARCH_DEV_TYPE               = 48,
    CCHEX_RET_UDP_SET_DEV_CONFIG_TYPE           = 49,

    CCHEX_RET_GET_INFOMATION_CODE_TYPE          = 50,
    CCHEX_RET_SET_INFOMATION_CODE_TYPE          = 51,
    CCHEX_RET_GET_BELL_INFO_TYPE                = 52,
    CCHEX_RET_SET_BELL_INFO_TYPE                = 53,
    CCHEX_RET_LIVE_SEND_ATTENDANCE_TYPE         = 54,
    CCHEX_RET_GET_USER_ATTENDANCE_STATUS_TYPE   = 55,
    CCHEX_RET_SET_USER_ATTENDANCE_STATUS_TYPE   = 56,
    CCHEX_RET_CLEAR_ADMINISTRAT_FLAG_TYPE       = 57,
    CCHEX_RET_GET_SPECIAL_STATUS_TYPE           = 58,
    CCHEX_RET_GET_ADMIN_CARD_PWD_TYPE           = 59,
    CCHEX_RET_SET_ADMIN_CARD_PWD_TYPE           = 60,
    CCHEX_RET_GET_DST_PARAM_TYPE                = 61,
    CCHEX_RET_SET_DST_PARAM_TYPE                = 62,
    CCHEX_RET_GET_DEV_EXT_INFO_TYPE             = 63,
    CCHEX_RET_SET_DEV_EXT_INFO_TYPE             = 64,
    CCHEX_RET_GET_BASIC_CFG3_TYPE               = 65,
    CCHEX_RET_SET_BASIC_CFG3_TYPE               = 66,
    CCHEX_RET_CONNECTION_AUTHENTICATION_TYPE    = 67,
    CCHEX_RET_GET_RECORD_NUMBER_TYPE            = 68,
    CCHEX_RET_GET_RECORD_BY_EMPLOYEE_TIME_TYPE  = 69,

    CCHEX_RET_GET_RECORD_INFO_STATUS_TYPE       = 70,
    CCHEX_RET_GET_NEW_RECORD_INFO_TYPE          = 71,

    CCHEX_RET_ULEMPLOYEE2W2_INFO_TYPE           = 72,


    CCHEX_RET_CLINECT_CONNECT_FAIL_TYPE         = 200,
};

/*******************************************************************************

*******************************************************************************/
//client by IP
typedef struct
{
    int Result; //0 ok, -1 err
    char Addr[ADDR_LEN];
} CCHEX_RET_DEV_CONNECT_STRU;

//get time
typedef struct
{
    unsigned int Year;
    unsigned int Month;
    unsigned int Day;
    unsigned int Hour;
    unsigned int Min;
    unsigned int Sec;
}CCHEX_MSG_GETTIME_STRU;  //24 bytes
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    CCHEX_MSG_GETTIME_STRU config;
} CCHEX_MSG_GETTIME_STRU_EXT_INF;

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char StartYear;
    unsigned char StartMonth;
    unsigned char StartDay;

    unsigned char EndYear;
    unsigned char EndMonth;
    unsigned char EndDay;
    //unsigned char Padding;
} CCHEX_MSGHEAD_INFO_STRU; // len 11   for  UNICODE and  ANSI

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char StartYear;
    unsigned char StartMonth;
    unsigned char StartDay;
    unsigned char StartHour;
    unsigned char StartMin;
    unsigned char StartSec;

    unsigned char EndYear;
    unsigned char EndMonth;
    unsigned char EndDay;
    unsigned char EndHour;
    unsigned char EndMin;
    unsigned char EndSec;

    //unsigned char Padding;
} CCHEX_MSGHEADUNICODE_INFO_STRU; // len 17 ,   for  SEAtS UNICODE

typedef struct
{
    unsigned char IpAddr[4];
    unsigned char IpMask[4];
    unsigned char MacAddr[6];
    unsigned char GwAddr[4];
    unsigned char ServAddr[4];
    unsigned char RemoteEnable;
    unsigned char Port[2];
    unsigned char Mode;
    unsigned char DhcpEnable;
} CCHEX_NETCFG_INFO_STRU; //27 bytes

/*******************************************************************************
    
*******************************************************************************/

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
} CCHEX_RET_COMMON_STRU;

#define CCHEX_RET_REBOOT_STRU                               CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SETTIME_STRU                              CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SETNETCFG_STRU                            CCHEX_RET_COMMON_STRU

#define CCHEX_RET_INIT_USER_AREA_STRU                       CCHEX_RET_COMMON_STRU
#define CCHEX_RET_INIT_SYSTEM_STRU                          CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_BASIC_CONFIG2_STRU                    CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_PERIOD_TIME_STRU                      CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_TEAM_INFO_STRU                        CCHEX_RET_COMMON_STRU
#define CCHEX_RET_ADD_FINGERPRINT_ONLINE_STRU               CCHEX_RET_COMMON_STRU
#define CCHEX_RET_FORCED_UNLOCK_STRU                        CCHEX_RET_COMMON_STRU
#define CCHEX_RET_UDP_SET_DEV_CONFIG_STRU                   CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_DEV_CONFIG_STRU_EXT_INF               CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_INFOMATION_CODE_STRU                  CCHEX_RET_COMMON_STRU

#define CCHEX_RET_SET_BELL_INFO_STRU                        CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_USER_ATTENDANCE_STATUS_INFO_STRU      CCHEX_RET_COMMON_STRU
#define CCHEX_RET_CLEAR_ADMINISTRAT_FLAG_STRU               CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_ADMIN_PWD_STRU                        CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_DST_PARAM_STRU                        CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_DEV_EXT_INFO_STRU                     CCHEX_RET_COMMON_STRU
#define CCHEX_RET_SET_BASIC_CONFIG3_STRU                    CCHEX_RET_COMMON_STRU
#define CCHEX_RET_CONNECTION_AUTHENTICATION_STRU            CCHEX_RET_COMMON_STRU

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    int Len;
} CCHEX_RET_MSGGETBYIDX_UNICODE_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    int Len;
} CCHEX_RET_MSGADDNEW_UNICODE_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    int Len;
} CCHEX_RET_MSGGETALLHEAD_UNICODE_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned char Idx;
} CCHEX_RET_MSGDELBYIDX_UNICODE_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned int TotalBytes;
    unsigned int SendBytes;
} CCHEX_RET_UPLOADFILE_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    CCHEX_NETCFG_INFO_STRU Cfg;
} CCHEX_RET_GETNETCFG_STRU;

#define SN_LEN 16
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned char sn[SN_LEN];
} CCHEX_RET_SN_STRU;

/*******************************************************************************
UDP  struct
*******************************************************************************/
typedef struct
{
    unsigned char IpAddr[4];
    unsigned char IpMask[4];
    unsigned char GwAddr[4];
    unsigned char MacAddr[6];
    unsigned char ServAddr[4];
    unsigned char Port[2];
    unsigned char NetMode;
} CCHEX_DEV_NET_INFO_STRU; //25 bytes
typedef struct
{
    unsigned char DevType[DEV_TYPE_NUMBER];
    unsigned char DevSerialNum[DEV_SERIAL_NUMBER];
    CCHEX_DEV_NET_INFO_STRU DevNetInfo;
    unsigned char Version[SOFTWARE_VERSION_LEN];
    unsigned char Reserved[4];

}CCHEX_UDP_SEARCH_STRU; //  63   ::  0:Dev without DNS;
typedef struct
{
    CCHEX_UDP_SEARCH_STRU BasicSearch;
    char Dns[4];
    char Url[100];
}CCHEX_UDP_SEARCH_WITH_DNS_STRU; //  167  ::  1:Dev has DNS;

typedef struct
{
    unsigned char CardName[10];
    unsigned char IpAddr[4];
    unsigned char IpMask[4];
    unsigned char GwAddr[4];
    unsigned char MacAddr[6];
}CCHEX_UDP_SEARCH_CARD_STRU; //28
typedef struct
{
    unsigned char DevType[DEV_TYPE_NUMBER];
    unsigned char DevSerialNum[DEV_SERIAL_NUMBER];
    unsigned char ServAddr[4];
    unsigned char Port[2];
    unsigned char NetMode;
    unsigned char Version[SOFTWARE_VERSION_LEN];
    unsigned char Reserved[4];
    unsigned char CardNumber;
    CCHEX_UDP_SEARCH_CARD_STRU CardInfo[2];
}CCHEX_UDP_SEARCH_TWO_CARD_STRU;//46+28*2 = 102    ::   2:Dev has two NetCard

typedef struct
{
    union
    {
        CCHEX_UDP_SEARCH_STRU WithoutDns;
        CCHEX_UDP_SEARCH_WITH_DNS_STRU WithDns;
        CCHEX_UDP_SEARCH_TWO_CARD_STRU TwoNetCard;
    };
    unsigned char Padding;

    int Result; //0: ok -1: fail
    unsigned int MachineId;
    int DevHardwareDataLen; //=63:Dev without DNS(CCHEX_UDP_SEARCH_STRU); = 173:Dev has DNS(CCHEX_UDP_SEARCH_WITH_DNS_STRU); 
                            // = 102:Dev has two NetCard(CCHEX_UDP_SEARCH_TWO_CARD_STRU).
    
}CCHEX_UDP_SEARCH_STRU_EXT_INF;//3*4+167+1 = 180
typedef struct
{
    int DevNum;
    CCHEX_UDP_SEARCH_STRU_EXT_INF dev_net_info[];
}CCHEX_UDP_SEARCH_ALL_STRU_EXT_INF;//4+DevNum*sizeof(CCHEX_UDP_SEARCH_STRU_EXT_INF)

typedef struct
{
    CCHEX_DEV_NET_INFO_STRU DevNetInfo;
    unsigned char Padding[3];
    unsigned int NewMachineId;
    unsigned char Reserved[4];
    unsigned char DevUserName[12];
    unsigned char DevPassWord[12];
    int DevHardwareType;//0:Dev without DNS;1:Dev has DNS;
    char Dns[4];
    char Url[100];
}CCHEX_UDP_SET_DEV_CONFIG_STRU_EXT_INF;//




/*******************************************************************************

*******************************************************************************/

typedef struct
{
    unsigned int MachineId; //机器号

    unsigned char NewRecordFlag;               //是否是新记录
    unsigned char EmployeeId[MAX_EMPLOYEE_ID]; //
    unsigned char Date[4];                     //日期时间
    unsigned char BackId;                      //备份号
    unsigned char RecordType;                  //记录类型

    unsigned char WorkType[3]; //工种        (ONLY use 3bytes )
    unsigned char Rsv;
} CCHEX_RET_RECORD_INFO_STRU;

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char Passwd[MAX_PWD_LEN];
    unsigned char CardId[MAC_CARD_ID_SMALL_LEN];
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_ASCII];

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned char FpStatus[FP_STATUS_LEN];
    unsigned char Special;
    unsigned char Padding;
} CCHEX_EMPLOYEE_INFO_STRU; //total 27 bytes +1 padding

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char Passwd[MAX_PWD_LEN];
    unsigned char CardId[MAC_CARD_ID_LEN];
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_ASCII];

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned char FpStatus[FP_STATUS_LEN];
    unsigned char PwdH8bit;
    unsigned char Rserved;
    unsigned char Special;
    unsigned char Padding[2];
} CCHEX_EMPLOYEE2_INFO_STRU; //total 30 bytes + 2 padding

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char Passwd[MAX_PWD_LEN];
    unsigned char CardId[MAC_CARD_ID_LEN];
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_UNICODE]; //unicode name

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned char FpStatus[FP_STATUS_LEN];
    unsigned char PwdH8bit;
    unsigned char Rserved;
    unsigned char Special;
} CCHEX_EMPLOYEE2UNICODE_INFO_STRU; //total 40 bytes   // for c2pro c2pronew

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char Passwd[MAX_PWD_LEN];
    unsigned char CardId[MAC_CARD_ID_LEN];
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_UNICODE]; //unicode name

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned char FpStatus[FP_STATUS_LEN];
    unsigned char PwdH8bit;
    unsigned char Rserved;
    unsigned char Special;
    unsigned char start_date[4];                //this sec  begin year is 2000.1.2
    unsigned char end_date[4];                  //this sec  begin year is 2000.1.2
}CCHEX_EMPLOYEE2W2_INFO_STRU;                   //48

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char Passwd[MAX_PWD_LEN];
    unsigned char CardId[MAC_CARD_ID_LEN];
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_UNICODE]; //unicode name

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned char FpStatus[FP_STATUS_LEN];
    unsigned char PwdH8bit;
    unsigned char Rserved;
    unsigned char Special;
    unsigned char EmployeeName2[MAX_EMPLOYEE_NAME_DR];
    unsigned char RFC[MAX_FRC_DR];
    unsigned char CURP[MAX_CURP_DR];
} CCHEX_EMPLOYEE2UNICODE_DR_INFO_STRU; //total 231 bytes   // for OA1000PM

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char Passwd[MAX_PWD_LEN];
    unsigned char CardId[MAC_CARD_ID_LEN];
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_22]; //unicode name

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned char FpStatus[FP_STATUS_LEN];
    unsigned char Rserved1;
    unsigned char Rserved2;
    unsigned char Special;
} CCHEX_EMPLOYEE3_INFO_STRU; //total 84 bytes   // for 761

typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE3_INFO_STRU Employee;
} CCHEX_RET_DLEMPLOYEE3_INFO_STRU;

// employee info for PC external interface
typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID]; // 5 bytes,12 digitals
    unsigned char password_len;
    unsigned char max_password;     // 6 digitals
    unsigned int password;          // 3 bytes,6 digitals
    unsigned char max_card_id;      // 6 digital for 3 bytes,10 digital for 4 bytes
    unsigned int card_id;           // 3 bytes, 6 digital; 4 types 10 digitals
    unsigned char max_EmployeeName; // 10, 20 , 64
    //unsigned char is_unicode;// 0:ASCII,1:unicode
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_22];

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned int Fp_Status; // 0~9:fp; 10:face; 11:iris1; 12:iris2
    unsigned char Rserved1; // for 22
    unsigned char Rserved2; // for 72 and 22
    unsigned char Special;
    unsigned char EmployeeName2[MAX_EMPLOYEE_NAME_DR];
    unsigned char RFC[MAX_FRC_DR];
    unsigned char CURP[MAX_CURP_DR];
} CCHEX_EMPLOYEE_INFO_STRU_EXT_INF; // new 282,old total 91 bytes

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID]; // 5 bytes,12 digitals
    unsigned char password_len;
    unsigned char max_password;     // 6 digitals
    unsigned int password;          // 3 bytes,6 digitals
    unsigned char max_card_id;      // 6 digital for 3 bytes,10 digital for 4 bytes
    unsigned int card_id;           // 3 bytes, 6 digital; 4 types 10 digitals
    unsigned char max_EmployeeName; // 10, 20 , 64
    //unsigned char is_unicode;// 0:ASCII,1:unicode
    unsigned char EmployeeName[MAX_EMPLOYEE_NAME_22];

    unsigned char DepartmentId;
    unsigned char GroupId;
    unsigned char Mode;
    unsigned int Fp_Status; // 0~9:fp; 10:face; 11:iris1; 12:iris2
    unsigned char Rserved1; // for 22
    unsigned char Rserved2; // for 72 and 22
    unsigned char Special;
    unsigned char EmployeeName2[MAX_EMPLOYEE_NAME_DR];
    unsigned char RFC[MAX_FRC_DR];
    unsigned char CURP[MAX_CURP_DR];
    unsigned char start_date[4];                //this sec  begin year is 2000.1.2
    unsigned char end_date[4];                  //this sec  begin year is 2000.1.2
} CCHEX_EMPLOYEE_INFO_STRU_EXT_INF_FOR_W2; 

typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE_INFO_STRU_EXT_INF Employee;
} CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF; // 294

typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE_INFO_STRU_EXT_INF_FOR_W2 Employee;
} CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_FOR_W2; 
typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE_INFO_STRU Employee;
} CCHEX_RET_DLEMPLOYEE_INFO_STRU;
typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE2_INFO_STRU Employee;
} CCHEX_RET_DLEMPLOYEE2_INFO_STRU;

typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE2UNICODE_INFO_STRU Employee;
} CCHEX_RET_DLEMPLOYEE2UNICODE_INFO_STRU;

typedef struct
{
    unsigned int MachineId;
    int CurIdx;
    int TotalCnt;

    CCHEX_EMPLOYEE2UNICODE_DR_INFO_STRU Employee;
} CCHEX_RET_DLEMPLOYEE2UNICODE_DR_INFO_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0: ok -1: fail

    CCHEX_EMPLOYEE_INFO_STRU Employee;
} CCHEX_RET_ULEMPLOYEE_INFO_STRU;
typedef struct
{
    unsigned int MachineId;
    int Result; //0: ok -1: fail

    CCHEX_EMPLOYEE2_INFO_STRU Employee;
} CCHEX_RET_ULEMPLOYEE2_INFO_STRU;
typedef struct
{
    unsigned int MachineId;
    int Result; //0: ok -1: fail
    CCHEX_EMPLOYEE2UNICODE_INFO_STRU Employee;
} CCHEX_RET_ULEMPLOYEE2UNICODE_INFO_STRU;
typedef struct
{
    unsigned int MachineId;
    int Result; //0: ok -1: fail
    CCHEX_EMPLOYEE2UNICODE_DR_INFO_STRU Employee;
} CCHEX_RET_ULEMPLOYEE2UNICODE_DR_INFO_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char FpIdx;
    unsigned int fp_len;
    unsigned char Data[FINGERPRINT_DATA_LEN_15360];
} CCHEX_RET_DLFINGERPRT_STRU;
#define CCHEX_RET_ULFINGERPRT_STRU CCHEX_RET_DLFINGERPRT_STRU

typedef struct
{
    int DevIdx;
    unsigned int MachineId;
    char Addr[ADDR_LEN];
    char Version[SOFTWARE_VERSION_LEN];
    char DevType[DEV_TYPE_LEN];
    int DevTypeFlag;
} CCHEX_RET_DEV_LOGIN_STRU;

typedef struct
{
    int DevIdx;
    unsigned int MachineId;
    unsigned int Live;
    char Addr[ADDR_LEN];
    char Version[SOFTWARE_VERSION_LEN];
    char DevType[DEV_TYPE_LEN];
} CCHEX_RET_DEV_LOGOUT_STRU;

typedef struct
{
    unsigned int MachineId;

    unsigned int EmployeeNum;
    unsigned int FingerPrtNum;
    unsigned int PasswdNum;
    unsigned int CardNum;
    unsigned int TotalRecNum;
    unsigned int NewRecNum;
} CCHEX_RET_DEV_STATUS_STRU;

// basic config info for PC external interface
typedef struct
{
    unsigned char software_version[SOFTWARE_VERSION_LEN];
    unsigned int password;
    unsigned char delay_for_sleep;
    unsigned char volume;
    unsigned char language;
    unsigned char date_format;
    unsigned char time_format;
    unsigned char machine_status;
    unsigned char modify_language;
    unsigned char cmd_version;
} CCHEX_GET_BASIC_CFG_INFO_STRU_EXT_INF; // 20 bytes

typedef struct
{
    unsigned int password;
    unsigned char pwd_len;
    unsigned char delay_for_sleep;
    unsigned char volume;
    unsigned char language;
    unsigned char date_format;
    unsigned char time_format;
    unsigned char machine_status;
    unsigned char modify_language;
    unsigned char reserved;
} CCHEX_SET_BASIC_CFG_INFO_STRU_EXT_INF; // 13 bytes

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    CCHEX_GET_BASIC_CFG_INFO_STRU_EXT_INF config;
} CCHEX_RET_GET_BASIC_CFG_STRU_EXT_INF;

// basic config 2 info for PC external interface
typedef struct
{
    unsigned char compare_level;            //
    unsigned char wiegand_range;
    unsigned char wiegand_type;
    unsigned char work_code;
    unsigned char real_time_send;
    unsigned char auto_update;
    unsigned char bell_lock;
    unsigned char lock_delay;
    unsigned int record_over_alarm;
    unsigned char re_attendance_delay;
    unsigned char door_sensor_alarm;
    unsigned char bell_delay;
    unsigned char correct_time;
} CCHEX_GET_BASIC_CFG_INFO2_STRU_EXT_INF; // 16 bytes

#define CCHEX_SET_BASIC_CFG_INFO2_STRU_EXT_INF CCHEX_GET_BASIC_CFG_INFO2_STRU_EXT_INF

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    CCHEX_GET_BASIC_CFG_INFO2_STRU_EXT_INF config;
} CCHEX_RET_GET_BASIC_CFG2_STRU_EXT_INF;

//Period of time
typedef struct
{
    unsigned char StartHour;
    unsigned char StartMin;
    unsigned char EndHour;
    unsigned char EndMin;
}CCHEX_GET_PERIOD_TIME_ONE_STRU_EXT_INF; //4
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    CCHEX_GET_PERIOD_TIME_ONE_STRU_EXT_INF day_week[7];
}CCHEX_GET_PERIOD_TIME_STRU_EXT_INF; //4+4+28 = 36
typedef struct
{
    unsigned char SerialNumbe;
    CCHEX_GET_PERIOD_TIME_ONE_STRU_EXT_INF day_week[7];
}CCHEX_SET_PERIOD_TIME_STRU_EXT_INF; //29

//Team Info
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned char PeriodTimeNumber[4];   
}CCHEX_GET_TEAM_INFO_STRU_EXT_INF;
typedef struct
{
    unsigned char TeamNumbe;
    unsigned char PeriodTimeNumber[4];
}CCHEX_SET_TEAM_INFO_STRU_EXT_INF;

//forced unlock
typedef struct
{
    unsigned char LockCmd;
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
}CCHEX_FORCED_UNLOCK_STRU_EXT_INF;
// del employee for PC interface
typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID]; // 5 bytes to save
    unsigned char backup;
} CCHEX_DEL_EMPLOYEE_INFO_STRU_EXT_INF; // 6 bytes
#define CCHEX_ADD_FINGERPRINT_ONLINE_STRU_EXT_INF CCHEX_DEL_EMPLOYEE_INFO_STRU_EXT_INF

// del record or new flag
typedef struct
{
    unsigned char del_type;
    unsigned int del_count;
} CCHEX_DEL_RECORD_OR_NEW_FLAG_INFO_STRU_EXT_INF; // 4 bytes

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned int deleted_count;
} CCHEX_RET_DEL_RECORD_OR_NEW_FLAG_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned int fp_len;                        //ANSI VERSION   fp_len == 10   UNICODE VERSION   fp_len == 20
    unsigned char info_code[20];                //infomation code
} CCHEX_RET_GET_INFOMATOIN_CODE_STRU;           //

typedef struct
{
    unsigned char hour;
    unsigned char minute;
    unsigned char flag_week;        //星期标志flag_week(用二进制0000000分别表示：六五四三二一日)
} CCHEX_RET_GET_BELL_TIME_POINT;
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    CCHEX_RET_GET_BELL_TIME_POINT time_point[30];
    unsigned char padding[2];
} CCHEX_RET_GET_BELL_INFO_STRU;

typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned char EmployeeId[MAX_EMPLOYEE_ID];
    unsigned char timestamp[4];                 //日期时间为相距2000年后的秒数2000.1.2
    unsigned char backup;                       //备分号
    unsigned char record_type;                  //记录类型
    unsigned char work_type[3];                 //工种
    unsigned char padding[2];
} CCHEX_RET_LIVE_SEND_ATTENDANCE_STRU;

typedef struct
{
    unsigned int fp_len;                        //data_info :: ANSI VERSION   fp_len = 80  UNICODE VERSION   fp_len = 160
    unsigned char atten_status_number;          //考勤状态数目 8 默认
    unsigned char data_info[160];               //数据格式:8组字符串  ANSI VERSION: unsigned char [8][10]  UNICODE VERSION: unsigned char[8][20]  
    unsigned char padding[3];                   //对齐 无效数据
} CCHEX_SET_USER_ATTENDANCE_STATUS_STRU;
typedef struct
{
    unsigned int MachineId;
    int Result;                                 //0 ok, -1 err
    unsigned int fp_len;                        //data_info :: ANSI VERSION   fp_len == 80  UNICODE VERSION   fp_len == 160
    unsigned char atten_status_number;          //考勤状态数目 == 8 默认
    unsigned char data_info[160];               //数据格式:  ANSI VERSION: unsigned char [8][10]  UNICODE VERSION: unsigned char[8][20] 
    unsigned char padding[3];
} CCHEX_RET_GET_USER_ATTENDANCE_STATUS_STRU;
//SPECIAL_STATUS
typedef struct
{
    unsigned int MachineId;
    int Result;
    unsigned char status;         //位1：门报警状态 0-正常状态 1-报警状态,位5：门状态 0-关闭 1-打开,位6：门磁状态 0-关闭 1-打开位,7：锁状态 0-关闭 1-打开
    unsigned char reserved[7];         //
} CCHEX_RET_GET_SPECIAL_STATUS_STRU;
//ADMIN_CARDNUM_PWD
typedef struct
{
    unsigned int MachineId;
    int Result;
    unsigned char data[13];
    unsigned char padding[3];
} CCHEX_RET_GET_ADMIN_CARDNUM_PWD_STRU;
//DST_PARAM
typedef struct
{
    unsigned char month;
    unsigned char day;
    unsigned char week_num;       //周次定义如下：0x01-0x04：前1-4周 0x81-0x82：后1-2周
    unsigned char flag_week;      //星期标志flag_week 0-6:(用二进制0000000分别表示：六五四三二一日)
    unsigned char hour;
    unsigned char minute;
    unsigned char sec;
} GET_DST_PARAM_TIME;   //7B
typedef struct
{
    unsigned char enabled;          //0-不启用      1-启用；
    unsigned char date_week_type;   //1-日期格式	2-星期格式；
    GET_DST_PARAM_TIME start_time;
    GET_DST_PARAM_TIME special_time;
} CCHEX_SET_DST_PARAM_STRU;         //16B
typedef struct
{
    unsigned int MachineId;
    int Result;
    CCHEX_SET_DST_PARAM_STRU param;
} CCHEX_RET_GET_DST_PARAM_STRU;

//DEV EXT INFO
typedef struct
{
    char manufacturer_name[50];         //厂商名称
    char manufacturer_addr[100];        //厂商地址
    char duty_paragraph[15];            //税号
    char reserved[155];                 //预留
} CCHEX_SET_DEV_EXT_INFO_STRU;  //320B
typedef struct
{
    unsigned int MachineId;
    int Result;
    CCHEX_SET_DEV_EXT_INFO_STRU param;
} CCHEX_RET_GET_DEV_EXT_INFO_STRU;

//basic config 3 info for PC external interface
typedef struct
{
    unsigned char wiegand_type;         //韦根读取方式
    unsigned char online_mode;
    unsigned char collect_level;
    unsigned char pwd_status;           //连接密码状态  =0 网络连接时不需要验证通讯密码 =1网络连接时需要先发送0x04命令 验证通讯密码
    unsigned char sensor_status;           //=0 不主动汇报门磁状态  =1主动汇报门磁状态（设备主动发送0x2F命令的应答包)
    unsigned char reserved[8];
    unsigned char independent_time;     
    unsigned char m5_t5_status;         //= 0	禁用 = 1	启用，本机状态为出=2	启用，本机状态为入 =4	禁用，本机状态为出 =5	禁用，本机状态为入
} CCHEX_SET_BASIC_CFG_INFO3_STRU;  //15B
typedef struct
{
    unsigned int MachineId;
    int Result;
    CCHEX_SET_BASIC_CFG_INFO3_STRU param;
    unsigned char padding;
} CCHEX_RET_GET_BASIC_CFG_INFO3_STRU;

//connection authentication
typedef struct
{
    unsigned char username[12];
    unsigned char password[12];
} CCHEX_CONNECTION_AUTHENTICATION_STRU;

//download by employeeId and time
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    int record_num;
} CCHEX_RET_GET_RECORD_NUMBER_STRU;

typedef struct
{
    unsigned char EmployeeId[MAX_EMPLOYEE_ID]; //员工号
    unsigned char start_date[4];                //相距2000年后的秒数2000.1.2
    unsigned char end_date[4];                  //相距2000年后的秒数2000.1.2
}CCHEX_GET_RECORD_INFO_BY_TIME;         //13B
typedef struct
{
    unsigned int MachineId;
    int Result; //0 ok, -1 err
    unsigned char EmployeeId[MAX_EMPLOYEE_ID]; //
    unsigned char date[4];                     //日期时间
    unsigned char back_id;                      //备份号
    unsigned char record_type;                  //记录类型
    unsigned char work_type[3]; //工种        (ONLY use 3bytes )
    unsigned char padding[2];
} CCHEX_RET_GET_EMPLOYEE_RECORD_BY_TIME_STRU;



API_EXTERN unsigned int CChex_Version();

API_EXTERN void CChex_Init();

/*****************************************************************************
    return AvzHandle
 *****************************************************************************/
API_EXTERN void *CChex_Start();

API_EXTERN void CChex_Stop(void *CchexHandle);
/************************************
 * after   CChex_Start();
 * para :
 *              SetRecordflag = 1  set  recordflag after download  new record;else = 0
 *              SetLogFile = 1     set some info  log to file for find  problem ;else = 0
 * if do not set "CChex_SetSdkConfig(void *CchexHandle,int SetRecordflag,int SetLogFile)", config is default   
 *                 ANVIZ_DEFAULT:
 *                                  W2   : SetRecordflag = 1,SetLogFile = 0;
 *                                  SEATS   : SetRecordflag = 1,SetLogFile = 0;
 *                                  DR   : SetRecordflag = 0,SetLogFile = 0;
 *                                  COMMON   : SetRecordflag = 0,SetLogFile = 0;
 ***************************************/
API_EXTERN void CChex_SetSdkConfig(void *CchexHandle,int SetRecordflag,int SetLogFile);
/*****************************************************************************
    EmployeeId size  = 5
    Data size = 338
*****************************************************************************/
API_EXTERN int CChex_UploadFingerPrint(void *CchexHandle, int DevIdx, unsigned char *EmployeeId, unsigned char FingerIdx, unsigned char *Data, unsigned int Data_len);
API_EXTERN int CChex_DownloadFingerPrint(void *CchexHandle, int DevIdx, unsigned char *EmployeeId, unsigned char FingerIdx);

API_EXTERN int CChex_UploadEmployeeInfo(void *CchexHandle, int DevIdx, CCHEX_EMPLOYEE_INFO_STRU *EmployeeList, unsigned char EmployeeNum);
API_EXTERN int CChex_UploadEmployee2Info(void *CchexHandle, int DevIdx, CCHEX_EMPLOYEE2_INFO_STRU *EmployeeList, unsigned char EmployeeNum);
API_EXTERN int CChex_UploadEmployee2UnicodeInfo(void *CchexHandle, int DevIdx, CCHEX_EMPLOYEE2UNICODE_INFO_STRU *EmployeeList, unsigned char EmployeeNum);
API_EXTERN int CChex_UploadEmployee2W2Info(void *CchexHandle, int DevIdx, CCHEX_EMPLOYEE2W2_INFO_STRU *EmployeeList, unsigned char EmployeeNum);
API_EXTERN int CChex_DownloadEmployeeInfo(void *CchexHandle, int DevIdx, unsigned int EmployeeCnt);

//FileType 0: firmware 1:pic 2: audio 3: language file
API_EXTERN int CChex_UploadFile(void *CchexHandle, int DevIdx, unsigned char FileType, char *FileName, char *Buff, int Len);

API_EXTERN int CChex_GetNetConfig(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetNetConfig(void *CchexHandle, int DevIdx, CCHEX_NETCFG_INFO_STRU *Config);

/*****************************************************************************
下载 全部记录   下载新记录
 *****************************************************************************/
API_EXTERN int CChex_DownloadAllRecords(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_DownloadAllNewRecords(void *CchexHandle, int DevIdx);
/*****************************************************************************

*****************************************************************************/
API_EXTERN int CChex_MsgGetByIdx(void *CchexHandle, int DevIdx, unsigned char Idx);
API_EXTERN int CChex_MsgDelByIdx(void *CchexHandle, int DevIdx, unsigned char Idx);
API_EXTERN int CChex_MsgGetAllHead(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_MsgAddNew(void *CchexHandle, int DevIdx, unsigned char *Data, int Len);

/*****************************************************************************

 *****************************************************************************/
API_EXTERN int CChex_RebootDevice(void *CchexHandle, int DevIdx);

/*****************************************************************************
sample 2017 9 30 10 10 10    --> set time 2017/09/10 10:10:10
 *****************************************************************************/
API_EXTERN int CChex_SetTime(void *CchexHandle, int DevIdx, int Year, int Month, int Day, int Hour, int Min, int Sec);
API_EXTERN int CChex_GetTime(void *CchexHandle, int DevIdx);

/*****************************************************************************

    return 
        >0  ok, return length
        =0, no result.
        <0  return (0 - need len)
 *****************************************************************************/
API_EXTERN int CChex_Update(void *CchexHandle, int *DevIdx, int *Type, char *Buff, int Len);

API_EXTERN int CChex_GetSNConfig(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetSNConfig(void *CchexHandle, int DevIdx, unsigned char *sn);

// 761
API_EXTERN int CChex_UploadEmployee3Info(void *CchexHandle, int DevIdx, CCHEX_EMPLOYEE3_INFO_STRU *EmployeeList, unsigned char EmployeeNum);
API_EXTERN int CChex_DownloadEmployee3Info(void *CchexHandle, int DevIdx, unsigned int EmployeeCnt);

API_EXTERN int CChex_GetBasicConfigInfo(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetBasicConfigInfo(void *CchexHandle, int DevIdx, CCHEX_SET_BASIC_CFG_INFO_STRU_EXT_INF *config);

API_EXTERN int CChex_ListPersonInfo(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_ModifyPersonInfo(void *CchexHandle, int DevIdx, CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF *EmployeeList, unsigned char EmployeeNum);
/****************************************************************
 *Just  for   DevType == "W2"
 ***************************************************************/
API_EXTERN int CChex_ModifyPersonW2Info(void *CchexHandle, int DevIdx, CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_FOR_W2 *EmployeeList, unsigned char EmployeeNum);
API_EXTERN int CChex_DeletePersonInfo(void *CchexHandle, int DevIdx, CCHEX_DEL_EMPLOYEE_INFO_STRU_EXT_INF *Employee);
API_EXTERN int CChex_DeleteRecordInfo(void *CchexHandle, int DevIdx, CCHEX_DEL_RECORD_OR_NEW_FLAG_INFO_STRU_EXT_INF *record);
/*****************************************************************************
    returned  =  1    Cmd Ok        = -1  Cmd Fail   
 *****************************************************************************/
API_EXTERN int CChex_InitUserArea(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_InitSystem(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_GetBasicConfigInfo2(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetBasicConfigInfo2(void *CchexHandle, int DevIdx, CCHEX_SET_BASIC_CFG_INFO2_STRU_EXT_INF *Config);
API_EXTERN int CChex_GetPeriodTime(void *CchexHandle, int DevIdx,unsigned char SerialNumbe);//(SerialNumbe==(1--32))
API_EXTERN int CChex_SetPeriodTime(void *CchexHandle, int DevIdx, CCHEX_SET_PERIOD_TIME_STRU_EXT_INF *Config);
API_EXTERN int CChex_GetTeamInfo(void *CchexHandle, int DevIdx,unsigned char TeamNumbe);//(TeamNumbe==(2--16))
API_EXTERN int CChex_SetTeamInfo(void *CchexHandle, int DevIdx, CCHEX_SET_TEAM_INFO_STRU_EXT_INF *Config);//(TeamNumbe==(2--16))
API_EXTERN int CCHex_AddFingerprintOnline(void *CchexHandle, int DevIdx, CCHEX_ADD_FINGERPRINT_ONLINE_STRU_EXT_INF *Param);
API_EXTERN int CCHex_Udp_Search_Dev(void *CchexHandle);
API_EXTERN int CCHex_Udp_Set_Dev_Config(void *CchexHandle, CCHEX_UDP_SET_DEV_CONFIG_STRU_EXT_INF *Config);//Config->DevHardwareType = 0:Dev without DNS; = 1:Dev has DNS;
/*****************************************************************************
  CCHEX_FORCED_UNLOCK_STRU_EXT_INF *Param = NULL  ;  just  Customized version :"Panasonic" use Param,
 *****************************************************************************/
API_EXTERN int CCHex_ForcedUnlock(void *CchexHandle, int DevIdx,CCHEX_FORCED_UNLOCK_STRU_EXT_INF *Param);




/***********************************
 * 功能 : 获取设置厂商信息码
 * 补充说明 : ANSI版本    信息码长度  10  UNICODE版本      信息码  20
************************************/
API_EXTERN int CChex_GetInfomationCode(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetInfomationCode(void *CchexHandle, int DevIdx,unsigned char *Data, unsigned int DataLen); //Data为 10 长度  或者 20长度的 字符数组

/***********************************
 * 功能 : 获取设置打铃信息
 * 参数 : FlagWeek 如果要设定星期一到星期五设定某时间段打铃，那么参数d的值就等于00111110=62
 *                  星期标志FlagWeek(用二进制0000000分别表示：六五四三二一日)
 * 返回值 : 
************************************/
API_EXTERN int CChex_GetBellInfo(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetBellInfo(void *CchexHandle, int DevIdx,unsigned char BellTimeNum,unsigned char Hour, unsigned char Min,unsigned char FlagWeek);

/***********************************
 * 功能 : 获取设置自定义考勤状态表
 * 补充说明 : ANSI版本    Data长度:  161  UNICODE版本      Data长度:  321
************************************/
API_EXTERN int CChex_GetUserAttendanceStatusInfo(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetUserAttendanceStatusInfo(void *CchexHandle, int DevIdx,CCHEX_SET_USER_ATTENDANCE_STATUS_STRU *Param);

/***********************************
 * 功能 : 清除管理员标志
************************************/
API_EXTERN int CChex_ClearAdministratFlag(void *CchexHandle, int DevIdx);

/***********************************
 * 功能 : 取特殊状态    VF30/VP30/T60+专用
************************************/
API_EXTERN int CChex_GetSpecialStatus(void *CchexHandle, int DevIdx);

/***********************************
 * 功能 : 读取管理卡号/管理密码        T5专用
 * 参数 :data[13]  : 如果机型为T5A，则为:     DATA	添加卡号	删除卡号	胁迫卡号	特殊信息
                                            Byte	 1-4	    5-8	      9-12	      13
                                            特殊信息定义如下:
        	                                位0：添加卡号
        	                                位1：删除卡号
                                            位2：胁迫卡号
                   : 如果机型为T50，则为:     DATA	管理密码长度+管理密码	    保留
                                            Byte	       1-3	             4-13
                                            管理密码长度 = Byte(1) >> 4
************************************/
API_EXTERN int CChex_GetAdminCardnumberPassword(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetAdminCardnumberPassword(void *CchexHandle, int DevIdx,unsigned char *Data,unsigned int DataLen); //Data[13] DataLen == 13

/***********************************
 * 功能 : 读取夏令时参数
************************************/
API_EXTERN int CChex_GetDSTParam(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetDSTParam(void *CchexHandle, int DevIdx,CCHEX_SET_DST_PARAM_STRU *Param);

/***********************************
 * 功能 : 获取机器扩展信息码
************************************/
API_EXTERN int CChex_GetDevExtInfo(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetDevExtInfo(void *CchexHandle, int DevIdx,CCHEX_SET_DEV_EXT_INFO_STRU *Param);

/***********************************
 * 功能 : 获取考勤机配置信息3	
************************************/
API_EXTERN int CChex_GetBasicConfigInfo3(void *CchexHandle, int DevIdx);
API_EXTERN int CChex_SetBasicConfigInfo3(void *CchexHandle, int DevIdx, CCHEX_SET_BASIC_CFG_INFO3_STRU *Config);

/***********************************
 * 功能 : 连接认证	CMD：0x04   虹膜设备用户 密码，其他设备，不验证用户名，只验证 通讯密码。
************************************/
API_EXTERN int CChex_ConnectionAuthentication(void *CchexHandle, int DevIdx,CCHEX_CONNECTION_AUTHENTICATION_STRU *Param);

/***********************************
 * 功能 : 按员工工号和时间获取考勤记录数量
************************************/
API_EXTERN int CChex_GetRecordNumByEmployeeIdAndTime(void *CchexHandle, int DevIdx,CCHEX_GET_RECORD_INFO_BY_TIME *Param);
API_EXTERN int CChex_DownloadRecordByEmployeeIdAndTime(void *CchexHandle, int DevIdx,CCHEX_GET_RECORD_INFO_BY_TIME *Param);


/***********************************
 * 功能 : 获取记录信息	
************************************/
API_EXTERN int CChex_GetRecordInfoStatus(void *CchexHandle, int DevIdx);

/*****************************************************************************
   Client: connect   returned  =  1    Cmd Ok        = -1  Cmd Fail
 *****************************************************************************/
API_EXTERN int CCHex_ClientConnect(void *CchexHandle, char *Ip, int Port);
API_EXTERN int CCHex_ClientDisconnect(void *CchexHandle,int DevIdx);









#ifdef __cplusplus
}
#endif

#endif
