#include <string.h>
#include <unistd.h>

static char *conservatorio_original_cwd = 0;

void xamarin_app_initialize (void *init_data)
{
	conservatorio_original_cwd = getcwd (NULL, 0);
}

extern char *conservatorio_get_original_cwd ()
{
	return strdup (conservatorio_original_cwd);
}
