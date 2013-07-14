#include <OpenGLES/ES1/gl.h>

extern "C" void UpdateTextureCPP(int srcNativeTexID, int dstNativeTexID)
{
    //glEnable(GL_TEXTURE_2D);
    glBindTexture(GL_TEXTURE_2D, dstNativeTexID);
	glCopyTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 0, 0, 1024, 1024, 0);
    //glDisable(GL_TEXTURE_2D);
}