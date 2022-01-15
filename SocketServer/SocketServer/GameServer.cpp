#include "GameServer.h"
#include <sys\types.h>
#include <sys\stat.h>
#include <Mmsystem.h>


GameServer::GameServer()
{
	portNum = 0;
	isRun = true;
	InitSocketLayer();

	for (int i = 0; i < 100; i++)
	{
		Clientinfo info;
		clientLists.push_back(info);
	}

	ClientIterator itr;
	itr = clientLists.begin();
	while (itr != clientLists.end())
	{
		clientPools.push(itr);
		++itr;
	}
}

GameServer::~GameServer()
{
	isRun = false;
	while (!clientPools.empty())clientPools.pop();

	ClientIterator itr;
	itr = clientLists.begin();
	while (itr != clientLists.end())
	{
		if (itr->isConnect == true)
		{
			WaitForSingleObject(itr->clientHandle, INFINITE);
			CloseHandle(itr->clientHandle);
			closesocket(itr->clientSock);
			++itr;
		}
	}
	WaitForSingleObject(listenHandle, INFINITE);
	CloseHandle(listenHandle);
	closesocket(serverSock);
	CloseSocketLayer();
}

int GameServer::InitSocketLayer()
{
	int retval = 0;
	WORD ver_request = MAKEWORD(2, 2);
	WSADATA wsa_data;

	if (WSAStartup(ver_request, &wsa_data))
	{
		printf("\SAStartup Error\n");
		return -1;
	}
	return 0;
}

void GameServer::CloseSocketLayer()
{
	WSACleanup();
}

void GameServer::Wait()
{
	WaitForSingleObject(listenHandle, INFINITE);
}

void GameServer::Listen(int nPort)
{
	portNum = nPort;
	listenHandle = (HANDLE)_beginthreadex(NULL, 0, GameServer::ListenThread, this, 0, NULL);

	if (!listenHandle)
	{
		printf("쓰레드 에러!\r\n");
		return;
	}
}

UINT WINAPI GameServer::ListenThread(LPVOID p)
{
	GameServer* pServer;
	pServer = (GameServer*)p;

	pServer->serverSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	if (pServer->serverSock == INVALID_SOCKET)
	{
		WSACleanup();
		return -1;
	}

	sockaddr_in service;
	service.sin_family = AF_INET;
	service.sin_addr.s_addr = INADDR_ANY;
	service.sin_port = htons(pServer->portNum);

	if (bind(pServer->serverSock, (SOCKADDR*)&service, sizeof(service)) == SOCKET_ERROR)
	{
		closesocket(pServer->serverSock);
		return -1;
	}

	if (listen(pServer->serverSock, 5) == SOCKET_ERROR)
	{
		closesocket(pServer->serverSock);
		return -1;
	}

	while (pServer->isRun)
	{
		SOCKET connectSock;
		connectSock = accept(pServer->serverSock, NULL, NULL);
		if (connectSock > 0)
		{
			if (pServer->clientPools.empty())
			{
				closesocket(connectSock);
				continue;
			}
			else
			{
				ClientIterator itr;
				itr = pServer->clientPools.top();
				pServer->clientPools.pop();
				itr->clientSock = connectSock;
				pServer->lastSock = connectSock;
				itr->isConnect = true;
				itr->clientHandle = (HANDLE)_beginthreadex(NULL, 0, GameServer::ControlThread, pServer, 0, NULL);
			}
		}
		Sleep(50);
	}
	return 0;
}

UINT WINAPI GameServer::ControlThread(LPVOID p)
{
	GameServer* pServer;
	pServer = (GameServer*)p;
	SOCKET connectSock = pServer->lastSock;

	bool bFound = false;

	ClientIterator itr;
	itr = pServer->clientLists.begin();
	while (itr != pServer->clientLists.end())
	{
		if (itr->clientSock == connectSock)
		{
			bFound = true;
			//char message[] = "20151687 김예찬!";
			//send(connectSock, message, sizeof(message), 0);
			printf("매칭 성공! 클라이언트연결 소켓%d 내부관리번호%s\n", itr->clientSock, itr->id);
			break;
		}
		++itr;
	}

	fd_set fdReadSet, fdErrorSet, fdMaster;
	struct timeval tvs;
	unsigned char cmd;
	unsigned char option;
	int len;
	char extraPacket[CVSP_STANDARD_PAYLOAD_LENGTH - sizeof(CVSPHeader_t)];
	FD_ZERO(&fdMaster);
	FD_SET(connectSock, &fdMaster);
	tvs.tv_sec = 0;
	tvs.tv_usec = 100;

	while(itr->isConnect&&bFound)	
	{
		fdReadSet = fdMaster;
		fdErrorSet = fdMaster;
		select((int)connectSock + 1, &fdReadSet, NULL, &fdErrorSet, &tvs);

		if (FD_ISSET(connectSock, &fdReadSet))
		{
			memset(extraPacket, 0, sizeof(extraPacket));
			//len = recv(connectSock, recvMessage, sizeof(recvMessage) - 1, 0);
			len = recvCVSP((unsigned int)connectSock, &cmd, &option, extraPacket, sizeof(extraPacket));
			if (len == SOCKET_ERROR)
			{
				printf("recv Error\n");
				break;
			}
			
			switch (cmd)
			{
			case CVSP_JOINREQ:
			{
				printf("클라이언트에서 아이디:%s\n", extraPacket);
 				sprintf_s(itr ->id, sizeof(itr->id), "%s", extraPacket);
				
				cmd = CVSP_JOINRES;
				option = CVSP_SUCCESS;
				if (sendCVSP((unsigned)itr->clientSock, cmd, option, NULL, 0) < 0)
				{
					printf("Send CVSP Error!\n");
				}
				ClientIterator citr;
				citr = pServer->clientLists.begin();
				while (citr != pServer->clientLists.end())
				{
					if (citr->isConnect && strcmp(citr->id, itr->id) != 0)
					{
						printf("citr%s itr%s\n", citr->id, itr->id);
						cmd = CVSP_JOINRES;
						option = CVSP_NEWUSER;
						if (sendCVSP((unsigned int)citr->clientSock, cmd, option, &itr->id, sizeof(itr->id)) < 0)
						{
							printf("Send CVSP Error!\n");
						}

						if (sendCVSP((unsigned int)itr->clientSock, cmd, option, &citr->id, sizeof(citr->id)) < 0)
						{
							printf("Send CVSP Error!\n");
						}
					}
					++citr;
				}	
				break;
			}	
			
			case CVSP_OPERATIONREQ:
			{
				PlayerInfo info;
				memcpy(&info, extraPacket, len);

				//info.id[strlen(extraPacket)] = '\0';

				ClientIterator citr;
				citr = pServer->clientLists.begin();
				while (citr != pServer->clientLists.end())
				{
					if (citr->isConnect && strcmp(citr->id, itr->id) != 0)
					{
						printf("%d %s [%f %f %f]\n",len, info.id, info.posX, info.posY, info.posZ);
						cmd = CVSP_MONITORINGMSG;
						option = CVSP_SUCCESS;
						if (sendCVSP((unsigned int)citr->clientSock, cmd, option, &info, sizeof(info)) < 0)
						{
							printf("Send CVSP Error!\n");
						}
					}
					++citr;
				}
			}
			break;
			case CVSP_CHATTINGREQ:
			{
				printf("클라이언트에서 받은 메시지:%s\n", extraPacket);
				//////////
				ClientIterator citr;
				citr = pServer->clientLists.begin();
				while (citr != pServer->clientLists.end())
				{
					if (citr->isConnect)
					{
						printf("메시지 재전송..클라이언트연결 소켓%d 내부관리번호%s\n", citr->clientSock, citr->id);
						cmd = CVSP_CHATTINGRES;
						option = CVSP_SUCCESS;
						if (sendCVSP((unsigned int)citr->clientSock,cmd,option,extraPacket,strlen(extraPacket))<0)
						{
							printf("Send Errorr!\n");
						}
					}

					++citr;
				}
			}
			break;
			case CVSP_ATTACK:
			{
				AttackInfo info;
				memcpy(&info, extraPacket, len);
				printf("CVSP_ATTACK [%s][%f, %f, %f]\n", info.id, info.posX, info.posY, info.PosZ);

				ClientIterator citr;
				citr = pServer->clientLists.begin();
				while (citr != pServer->clientLists.end())
				{
					if (citr->isConnect && strcmp(citr->id, itr->id) != 0)
					{
						cmd = CVSP_ATTACK;
						option = CVSP_SUCCESS;
						
						if (sendCVSP((unsigned int)citr->clientSock, cmd, option, &info, sizeof(info)) < 0)
						{
							printf("Send CVSP Error!\n");
						}
					}
					citr++;
				}
			}
			break;
			case CVSP_DIE:
			{
				HPInfo info;
				memcpy(&info, extraPacket, len);
				printf("CVSP_DIE [%s][%f]\n", info.id, info.hp);

				ClientIterator citr;
				citr = pServer->clientLists.begin();
				while (citr != pServer->clientLists.end())
				{
					if (citr->isConnect && strcmp(citr->id, itr->id) != 0)
					{
						cmd = CVSP_DIE;
						option = CVSP_SUCCESS;

						if (sendCVSP((unsigned int)citr->clientSock, cmd, option, &info, sizeof(info)) < 0)
						{
							printf("Send CVSP Error!\n");
						}
					}
					citr++;
				}
			}
			break;
			case CVSP_SCORE:
			{
				ScoreInfo info;
				memcpy(&info, extraPacket, len);
				printf("CVSP_DIE [%s][%d][%d]\n", info.id, info.kills,info.deaths);

				ClientIterator citr;
				citr = pServer->clientLists.begin();
				while (citr != pServer->clientLists.end())
				{
					if (citr->isConnect && strcmp(citr->id, itr->id) != 0)
					{
						cmd = CVSP_SCORE;
						option = CVSP_SUCCESS;

						if (sendCVSP((unsigned int)citr->clientSock, cmd, option, &info, sizeof(info)) < 0)
						{
							printf("Send CVSP Error!\n");
						}
					}
					citr++;
				}
			}
			break;
			case CVSP_LEAVEREQ:
				printf("소켓 연결을 종료 합니다..\n");
				itr->isConnect = false;
				break;
			}
		}

	}	
	
	closesocket(itr->clientSock);
	pServer->clientPools.push(itr);
	printf("Client Connection Exited!\n");

	return 0;
}