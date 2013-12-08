#ifndef SOCKET_H
#define SOCKET_H
#include <string>
#include "global.h"

#define IPAddress struct sockaddr_in
using std::string;

class Socket
{
	public:
		Socket();
		Socket(int family, int type, int protocol);
		Socket(const Socket&);
		virtual ~Socket();
		void Bind(IPAddress *address);
		void Listen(int backlog);
		Socket Accept();
		void Connect(IPAddress *address);
		void Close();
		size_t Receive(char *buff, size_t bytes, int flags);
		size_t Send(const char *buff, size_t bytes, int flags);
		string GetClientIP();
		in_port_t GetClientPort();
		string GetLocalIP();
		in_port_t GetLocalPort();
		int GetAddressFamily() { return addressFamily; }
		int GetSocketType() { return socketType; }
		int GetProtocolType() { return protocolType; }
		Socket& operator=(const Socket&);

		int socketfd;
	private:
		IPAddress clientAddress;
		IPAddress localAddress;
		int addressFamily;
		int socketType;
		int protocolType;
};
#endif
