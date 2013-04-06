
all: HalfMoon.msi

HalfMoon.msi: HalfMoon.wixobj
        light -ext WixNetFxExtension -nologo HalfMoon.wixobj
        
HalfMoon.wixobj: HalfMoon.wxs
        candle -nologo HalfMoon.wxs
        
clean:
        del *.wixobj
        del *.wixpdb
        del *.msi
