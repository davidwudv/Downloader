/*
 * =====================================================================================
 *
 *       Filename:  Socket.cpp
 *
 *    Description:  此类还需要完善，应该为socketfd定义一个智能指针类，这样才能保证Socket
 *					类在赋值、复制后，还能正确的关闭相关套接字；
 *
 *        Version:  1.0
 *        Created:  2013年12月01日 22时06分35秒
 *       Revision:  none
 *       Compiler:  gcc
 *
 *         Author:  dvwei (吴德伟), davidwudv@gmail.com | dvwei@outlook.com
 *   Organization:  
 *
 * =====================================================================================
 */
#include "Socket.h"
//#include "global.h"
#include <string.h>

Socket::Socket():
	socketfd(-1), addressFamily(-1), socketType(-1), protocolType(-1) 
{
	bzero(&clientAddress, sizeof(clientAddress));
	bzero(&localAddress, sizeof(localAddress));
}

Socket::Socket(int family, int type, int protocol): 
	socketfd(-1), addressFamily(family), socketType(type), protocolType(protocol)
{
	if((socketfd = socket(family, type, protocol)) < 0)
	  err_sys("socket error");
	bzero(&clientAddress, sizeof(clientAddress));
	bzero(&localAddress, sizeof(localAddress));

}

Socket::Socket(const Socket &srcSocket)
{
	socketfd = srcSocket.socketfd;
	addressFamily = srcSocket.addressFamily;
	socketType = srcSocket.socketType;
	protocolType = srcSocket.protocolType;
	bcopy(&srcSocket.clientAddress, &clientAddress, sizeof(srcSocket.clientAddress));
	bcopy(&srcSocket.localAddress, &localAddress, sizeof(srcSocket.localAddress));
}

Socket::~Socket()
{
	if(socketfd != -1)
	  close(socketfd);
}

void Socket::Bind(IPAddress *address)
{
	socklen_t addrlen = sizeof(*address);
	if(bind(socketfd, (struct sockaddr *)address, addrlen) < 0)
	  err_sys("bind error");
}

void Socket::Listen(int backlog)
{
	char *ptr;
	if( (ptr = getenv("LISTENQ")) != NULL)
	  backlog = atoi(ptr);
	if(listen(socketfd, backlog) < 0)
	  err_sys("listen error");
}

Socket Socket::Accept()
{
	socklen_t addrlen = sizeof(clientAddress);
	int acceptSocketfd;
again:
	if((acceptSocketfd = accept(socketfd, (struct sockaddr *)&clientAddress, &addrlen)) < 0)
	{
#ifdef EPROTO
		if(errno == EPROTO || errno == ECONNABORTED)
#else
		if(errno == ECONNABORTED)
#endif
		  goto again;
		else
		  err_sys("accept error");
	}

	Socket connectSocket(*this);
	connectSocket.socketfd = acceptSocketfd;
	return connectSocket;
}

void Socket::Connect(IPAddress *address)
{
	socklen_t addrlen = sizeof(*address);
	if(connect(socketfd, (struct sockaddr *)address, addrlen) < 0)
	  err_sys("connect error");
}

void Socket::Close()
{
	if(socketfd != -1)
	{
		close(socketfd);
		socketfd = -1;
	}
}

size_t Socket::Receive(char *buff, size_t bytes, int flags = 0)
{
	return recv(socketfd, buff, bytes, flags);
}

size_t Socket::Send(const char *buff, size_t bytes, int flags = 0)
{
	return send(socketfd, buff, bytes, flags);
}

string Socket::GetClientIP()
{
	char strIP[200];
	inet_ntop(addressFamily, &clientAddress.sin_addr, strIP, sizeof(strIP));
	string IP(strIP);
	return IP;
}

string Socket::GetLocalIP()
{
	char strIP[200];
	inet_ntop(addressFamily, &localAddress.sin_addr, strIP, sizeof(strIP));
	string IP(strIP);
	return IP;
}

in_port_t Socket::GetClientPort()
{
	return ntohs(clientAddress.sin_port);
}

in_port_t Socket::GetLocalPort()
{
	return ntohs(localAddress.sin_port);
}

Socket& Socket::operator=(const Socket &rhs)
{
	socketfd = rhs.socketfd;
	addressFamily = rhs.addressFamily;
	socketType = rhs.socketType;
	protocolType = rhs.protocolType;
	bcopy(&rhs.clientAddress, &clientAddress, sizeof(rhs.clientAddress));
	bcopy(&rhs.localAddress, &localAddress, sizeof(rhs.localAddress));
	const_cast<Socket&>(rhs).socketfd = -1;//非常不好的代码，因为暂时懒得定义智能指针类，所以临时使用这种做法。
	return *this;
}
