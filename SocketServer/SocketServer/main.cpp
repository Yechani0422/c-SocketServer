#include "GameServer.h"

int main()
{
	GameServer server;
	server.Listen(5004);
	server.Wait();

	return 0;
}