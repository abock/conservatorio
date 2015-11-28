XM_PATH = /Library/Frameworks/Xamarin.Mac.framework/Versions/Current
BMAC = $(XM_PATH)/bin/bmac

.PHONY: all
all: bind

.PHONY: bootstrap
bootstrap:
	sharpie bind -f ../external/Sparkle/build/Release/Sparkle.framework -n Sparkle

.PHONY: bind
bind: Sparkle.dll

Sparkle.dll: ApiDefinitions.cs StructsAndEnums.cs Constants.cs
	$(BMAC) -out:$@ -api:ApiDefinitions.cs StructsAndEnums.cs --unified-mobile-profile --outdir:build -x:Constants.cs

.PHONY: clean
clean:
	rm -f Sparkle.dll Sparkle.dll.mdb
	rm -rf build
