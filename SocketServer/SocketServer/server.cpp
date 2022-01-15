#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

#include <stdio.h>
#include <stdlib.h>
#include <WinSock2.h>
#include <process.h>

unsigned __stdcall Run(void* p);
bool g_IsRunning;

int main(int argc, char** argv)
{
	WSADATA wsadata;
	SOCKET ServSock,CliSock;
	SOCKADDR_IN servAddr,cliAddr;
	int portnum;
	int szCliAddr;
	int len;
	portnum = 5004;
	HANDLE hThread;

	char message[] = "20151687_김예찬!";
	char sendMessage[100];

	if (WSAStartup(MAKEWORD(2, 2), &wsadata) != 0)
	{
		printf("WSAStartup Error\n");
		return -1;
	}	

	ServSock = socket(PF_INET, SOCK_STREAM, 0);
	
	if (ServSock == INVALID_SOCKET)
	{
		printf("socket 생성 실패\n");
		return -1;
	}

	memset(&servAddr, 0, sizeof(servAddr));
	servAddr.sin_family = AF_INET;
	servAddr.sin_port = htons(portnum);
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	if (bind(ServSock, (SOCKADDR*)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
	{
		printf("bind() ERROR\n");
		return -1;
	}

	if (listen(ServSock, 5) == SOCKET_ERROR)
	{
		printf("listen() ERROR\n");
		return -1;
	}
	szCliAddr = sizeof(cliAddr);

	CliSock = accept(ServSock, (SOCKADDR*)&cliAddr, &szCliAddr);

	if (CliSock == INVALID_SOCKET)
	{
		printf("accept() ERROR\n");
		return -1;
	}
	else
	{
		printf("CONNECTION SUCCESS . . . . . . .\n");
	}

	send(CliSock, message, sizeof(message), 0);
	//추가
	hThread = (HANDLE)_beginthreadex(NULL, 0, Run, (void*)CliSock, CREATE_SUSPENDED, NULL);
	if (!hThread)
	{
		printf("쓰레드에러!\r\n");
		return -1;
	}
	g_IsRunning = true;
	ResumeThread(hThread);

	while (g_IsRunning==true)
	{		
		printf("클라이언트로 전송할 메시지입력:");
		gets_s(sendMessage, sizeof(sendMessage));

		send(CliSock, sendMessage, (int)strlen(sendMessage), 0);

		if (strcmp(sendMessage, "exit") == 0)
		{
			g_IsRunning = false;
			break;
		}
	}
	WaitForSingleObject(hThread, INFINITE);
	CloseHandle(hThread);
	closesocket(CliSock);
	closesocket(ServSock);


	if (WSACleanup() == SOCKET_ERROR)
	{
		printf("WSACleanup faild with error %d\n", WSAGetLastError());
		return -1;
	}

	return 0;
}


unsigned __stdcall Run(void* p)
{
	SOCKET CliSock = (SOCKET)p;
	char recvMessage[100];
	int len;

	while (g_IsRunning == true)
	{
		memset(recvMessage, 0, sizeof(recvMessage));
		len = recv(CliSock, recvMessage, sizeof(recvMessage) - 1, 0);
		if (len == SOCKET_ERROR)
		{
			printf("recv Error\n");
			break;
		}
		recvMessage[len] = '\0';

		if (strcmp(recvMessage, "exit") == 0)
		{
			printf("소켓 연결을 종료 합니다..\n");
			g_IsRunning = false;
			break;
		}
		printf("클라이언트에서 받은 메시지:%s\n", recvMessage);
	}

	return 0;
}

