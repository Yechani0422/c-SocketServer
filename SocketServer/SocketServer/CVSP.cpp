#include "CVSP.h"

int sendCVSP(unsigned int sockfd, unsigned char cmd, unsigned char option, void* payload, unsigned short len)
{
	char* CVSPPacket;
	CVSPHeader_t CVSPHeader;
	u_int packetSize;
	int result;

	packetSize = len + sizeof(CVSPHeader_t);

	CVSPHeader.cmd = cmd;
	CVSPHeader.option = option;
	CVSPHeader.packetLength = packetSize;

	CVSPPacket = (char*)malloc(packetSize);
	assert(CVSPPacket);

	memset(CVSPPacket, 0, packetSize);
	memcpy(CVSPPacket, &CVSPHeader, sizeof(CVSPHeader_t));

	if (payload != NULL)
	{
		memcpy(CVSPPacket + sizeof(CVSPHeader_t), payload, len);
	}

	result = send(sockfd, CVSPPacket, packetSize, 0);
	if (result < 0)
	{
		return result;
	}
	free(CVSPPacket);
	return result;
}

int recvCVSP(unsigned int sockfd, unsigned char* cmd, unsigned char* option, void* payload, unsigned short len)
{
	CVSPHeader_t CVSPHeader;
	char extraPacket[CVSP_STANDARD_PAYLOAD_LENGTH];
	int recvSize;
	int payloadSize;
	int payloadCopySize;
	assert(payload != NULL);
	memset(extraPacket, 0, sizeof(extraPacket));

	int last_readn, cur_readn;
	int ret = 0;
	recvSize = recv(sockfd, (char*)&CVSPHeader, sizeof(CVSPHeader), MSG_PEEK);

	if (recvSize < 0)
	{
		return recvSize;
	}
	last_readn = CVSPHeader.packetLength;
	cur_readn = 0;

	for (; cur_readn != CVSPHeader.packetLength;)
	{
		ret = recv(sockfd, extraPacket + cur_readn, last_readn, 0);
		if (ret < 0)
		{
			return -1;
		}
		last_readn -= ret;
		cur_readn += ret;
	}

	memcpy(&CVSPHeader, extraPacket, sizeof(CVSPHeader));
	payloadSize = CVSPHeader.packetLength - sizeof(CVSPHeader_t);
	*cmd = CVSPHeader.cmd;
	*option = CVSPHeader.option;
	payloadCopySize = payloadSize;

	if (payloadCopySize != 0)
	{
		memcpy(payload, extraPacket + sizeof(CVSPHeader), payloadCopySize);
	}
	return payloadCopySize;
}