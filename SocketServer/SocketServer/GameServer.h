#pragma once

#pragma comment(lib,"ws2_32")
#pragma warning(disable:4996)

#include <stdio.h>
#include <stdlib.h>
#include <WinSock2.h>
#include <process.h>
#include <iostream>
#include <vector>
#include <stack>
#include "CVSP.h"

struct Clientinfo
{
	SOCKET clientSock;
	bool isConnect;
	char id[50];
	HANDLE clientHandle;
	HANDLE listenHandle;

	Clientinfo()
	{
		memset(id, 0, sizeof(id));
		clientSock = NULL;
		isConnect = false;
	}
};



typedef std::vector<Clientinfo>::iterator ClientIterator;

class GameServer
{
	int portNum;
	SOCKET serverSock;
	HANDLE listenHandle;
	HANDLE mainHandle;
	bool isRun;
	SOCKET lastSock;

	std::vector<Clientinfo> clientLists;
	std::stack<ClientIterator, std::vector<ClientIterator>>clientPools;
private:
	int InitSocketLayer();
	void CloseSocketLayer();
public:
	GameServer();
	~GameServer();
	void Wait();
	void Listen(int nPort);
	static UINT WINAPI ListenThread(LPVOID p);
	static UINT WINAPI ControlThread(LPVOID p);
};

