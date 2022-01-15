#pragma once

// Collaborative Virtual Service Protocol(CVSP)
// This is an application level protocol using TCP.

#ifndef _CVSP_1_0_0_H_
#define _CVSP_1_0_0_H_

#include <stdio.h>
#include <assert.h>
#include <malloc.h>
#include <string.h>
#include <stdlib.h>
#include <fcntl.h>

#define WIN32 1

#ifdef WIN32

#pragma comment(lib, "ws2_32.lib")
#include <winsock.h>
#include <io.h>
#endif

#ifndef WIN32

#include <sys/socket.h>
#include <unistd.h>

#endif

#define CVSP_MONITORING_MESSAGE			700
#define CVSP_MONITORING_LOAD			701

// version 
#define CVSP_VER						0x01
// Port	
#define CVSP_PORT						9000
// payload size 
#define CVSP_STANDARD_PAYLOAD_LENGTH	4096
// Command 
#define CVSP_JOINREQ					0x01
#define CVSP_JOINRES					0x02
#define CVSP_CHATTINGREQ				0x03
#define CVSP_CHATTINGRES				0x04
#define CVSP_OPERATIONREQ				0x05
#define CVSP_MONITORINGMSG				0x06
#define CVSP_LEAVEREQ					0x07
//공격 과 히트 리스폰 정의
#define CVSP_ATTACK                     0x08
#define CVSP_DIE                        0x09
#define CVSP_SCORE                      0x10


// option  
#define CVSP_SUCCESS					0x01
#define CVSP_FAILE						0x02
#define CVSP_NEWUSER                    0x03


/* variable style */
#ifdef WIN32
typedef unsigned char  u_char;
typedef unsigned int   u_int;
typedef unsigned short u_short;
typedef unsigned long  u_long;
#endif	
/*	Header(32bit)
		|0   |8	   |12	|16      |24     |
		|____|_____|______|______|_______|____________________|
		|Ver  |cmd |Option|Packet|Length |PayLoad			  |
		|__________|______|______|_______|____________________|
*/
typedef struct CVSPHeader_t {
	u_char			cmd;
	u_char			option;
	u_short			packetLength;
} CVSPHeader;

// #define MFCSOCK

// Version
#define CVSP_VER			0x01

// API interface
int sendCVSP(unsigned int sockfd, unsigned char cmd, unsigned char option, void* payload, unsigned short len);
int recvCVSP(unsigned int sockfd, unsigned char* cmd, unsigned char* option, void* payload, unsigned short len);

// 여기에 통신할 데이터들을 정의할 수 있음
struct PlayerInfo
{
	char id[50];
	float posX;
	float posY;
	float posZ;
	float quatX;
	float quatY;
	float quatZ;
	float quatW;
};

//공격 메시지를 처리하기 위한 구조체
struct AttackInfo
{
	char id[50];
	float posX;
	float posY;
	float PosZ;
	float quatX;
	float quatY;
	float quatZ;
	float quatW;
};

struct HPInfo
{
	char id[50];
	char bulletOwner[50];
	int hp;
};

struct ScoreInfo
{
	char id[50];
	int kills;
	int deaths;
};

#endif