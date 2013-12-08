/*
 * =====================================================================================
 *
 *       Filename:  main.cpp
 *
 *    Description:  
 *
 *        Version:  1.0
 *        Created:  2013年12月02日 18时18分34秒
 *       Revision:  none
 *       Compiler:  gcc
 *
 *         Author:  dvwei (吴德伟), davidwudv@gmail.com | dvwei@outlook.com
 *   Organization:  
 *
 * =====================================================================================
 */

#include <iostream>
#include <syslog.h>
#include <fcntl.h>
#include <sys/resource.h>
#include <sys/stat.h>
#include "Socket.h"
#include "global.h"

using namespace std;

bool print_out = false;
void daemonize(const char *cmd);

int main(int argc, char **argv)
{
	if(argc == 2) 
	{
		if(strcmp(argv[1],"-b") == 0)
		  daemonize(argv[0]);
		else
		{
			cout << "无效的参数！" << endl;
			exit(0);
		}
	}
	else if(argc > 2)
	{
		cout << "参数个数超过限制!" << endl;
		exit(0);
	}
	else
	  print_out = true;
	pid_t pid;
	Socket listenSocket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	struct sockaddr_in localAddress;
	char buff[BUFFSIZE + 1];

	bzero(&localAddress, sizeof(localAddress));
	localAddress.sin_family = AF_INET;
	localAddress.sin_addr.s_addr = htonl(INADDR_ANY);
	localAddress.sin_port = htons(9394);

	listenSocket.Bind(&localAddress);
	listenSocket.Listen(1024);

	Signal(SIGCHLD, sig_child);
	Signal(SIGPIPE, sig_pipe);
	while(true)
	{
		int receivedSize(0);
		Socket connectSocket(listenSocket.Accept());
		if(print_out)
		  cout << "connection from " << connectSocket.GetClientIP()
			<< ", port " << connectSocket.GetClientPort() << endl;
		if( (pid = fork()) < 0)
		  err_sys("fork error");
		else if(pid == 0)//in child process
		{

			listenSocket.Close();
			receivedSize = connectSocket.Receive(buff, BUFFSIZE, 0);
			if(print_out)
			  cout << "Received " << receivedSize << " bytes request from " << connectSocket.GetClientIP()
				<< ":" << connectSocket.GetClientPort() << endl;
			if(receivedSize > 0)
			{
				string requestString(buff);
				if(requestString.find("HTTP/1.1\r\n") > 0)//HTTP
				{
					int hostStartIndex = requestString.find("Host: ");
					int hostEndIndex = requestString.find_first_of("\r\n", hostStartIndex);
					string hostName = requestString.substr(hostStartIndex + 6, hostEndIndex - hostStartIndex - 6);
					struct hostent *hostEntry;
					if( (hostEntry = gethostbyname(hostName.c_str())) == NULL)
					  err_sys("gethostbyname error");
					Socket downloadServerSocket(AF_INET, SOCK_STREAM, 0);
					struct sockaddr_in downloadServerAddr;
					char ip[1024];
					bzero(&downloadServerAddr, sizeof(downloadServerAddr));
					downloadServerAddr.sin_family = AF_INET;
					downloadServerAddr.sin_port = htons(80);
					inet_pton(AF_INET, inet_ntop(AF_INET, hostEntry->h_addr_list[0], ip, sizeof(ip)), &downloadServerAddr.sin_addr);
					downloadServerSocket.Connect(&downloadServerAddr);
					downloadServerSocket.Send(buff, receivedSize, 0);
					receivedSize = downloadServerSocket.Receive(buff, BUFFSIZE, 0);
					if(print_out)
					{
						cout << "Received " << receivedSize << " bytes from download server(" 
							<< inet_ntop(AF_INET, &downloadServerAddr.sin_addr, ip, sizeof(ip))
							<< ":80)" << endl;
						cout << "begin received from " << ip << " and send to " << connectSocket.GetClientIP() << endl;
					}
					while(receivedSize > 0)
					{
						connectSocket.Send(buff, receivedSize, 0);
						receivedSize = downloadServerSocket.Receive(buff, BUFFSIZE, 0);
#ifdef DEBUG
					//	cout << "Received " << receivedSize << " bytes from " 
					//		<< inet_ntop(AF_INET, &downloadServerAddr.sin_addr, ip, sizeof(ip))
					//		<< ":80" << endl;
#endif
					}
					downloadServerSocket.Close();
					exit(0);

				}
				else//此处添加对其他协议的处理
				{
					return 0;
				}
			}
		connectSocket.Close();

		}
	}
	//listenSocket.Close();
	
	return 0;
}

void daemonize(const char *cmd)//以守护进程运行
{
	int					i, fd0, fd1, fd2;
	pid_t				pid;
	struct rlimit		rl;
	struct sigaction	sa;

	umask(0);
	if(getrlimit(RLIMIT_NOFILE, &rl) < 0)
	  err_quit("%s: can't get file limit", cmd);
	if( (pid = fork()) < 0)
	  err_quit("%s: can't fork", cmd);
	else if(pid != 0) //parent process
	  exit(0);
	setsid();

	sa.sa_handler = SIG_IGN;
	sigemptyset(&sa.sa_mask);
	sa.sa_flags = 0;
	if(sigaction(SIGHUP, &sa, NULL) < 0)
	  err_quit("%s: can't ignore SIGHUP");
	if( (pid = fork()) < 0)
	  err_quit("%s: can't fork", cmd);
	else if(pid != 0)//parent process
	  exit(0);
	
	if(chdir("/") < 0)
	  err_quit("%s: can't change directory to /");
	if(rl.rlim_max == RLIM_INFINITY)
	  rl.rlim_max = 1024;
	for(i = 0; i < rl.rlim_max; i++)
	  close(i);

	fd0 = open("/dev/null", O_RDWR);
	fd1 = dup(0);
	fd2 = dup(0);

	openlog(cmd, LOG_CONS, LOG_DAEMON);
	if(fd0 != 0 || fd1 != 1 || fd2 != 2)
	{
		syslog(LOG_ERR, "unexpected file descriptors %d %d %d", fd0, fd1, fd2);
		exit(1);
	}
}
