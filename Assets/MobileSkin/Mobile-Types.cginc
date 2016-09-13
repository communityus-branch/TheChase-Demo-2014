
#ifndef MOBILE_TYPES_INCLUDE
#define MOBILE_TYPES_INCLUDE

#if 1 // prefer fixed (Tegra)
#	define color fixed
#	define color2 fixed2
#	define color3 fixed3
#	define color4 fixed4
#	define normal3 fixed3
#else
#	define color half
#	define color2 half2
#	define color3 half3
#	define color4 half4
#	define normal3 half3
#endif

#define coord half
#define coord2 half2
#define coord3 half3
#define coord4 half4

#endif