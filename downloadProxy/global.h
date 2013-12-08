#ifndef GLOBAL_H
#define GLOBAL_H
#include <errno.h>
#include <cstdlib>
#include <sys/socket.h>
#include <strings.h>
#include <cstring>
#include <netdb.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <signal.h>
#include <sys/wait.h>

#define BUFFSIZE 5120 //5KB

extern bool print_out;

void err_ret(const char *fmt, ...);
void err_sys(const char *fmt, ...);
void err_dump(const char *fmt, ...);
void err_msg(const char *fmt, ...);
void err_quit(const char *fmt, ...);
typedef	void	Sigfunc(int);	/* for signal handlers */
Sigfunc *signal(int signo, Sigfunc *func);
Sigfunc *Signal(int signo, Sigfunc *func);
void sig_child(int signo);
void sig_pipe(int signo);
#endif
