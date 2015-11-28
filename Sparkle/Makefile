XM_PATH = /Library/Frameworks/Xamarin.Mac.framework/Versions/Current
BMAC = $(XM_PATH)/bin/bmac

.PHONY: all
all: bind

.PHONY: bootstrap
bootstrap:
	sharpie bind -f ../external/Sparkle/build/Release/Sparkle.framework -n Sparkle

.PHONY: bind
bind:
	$(BMAC) -out:Sparkle.dll -api:ApiDefinitions.cs StructsAndEnums.cs --unified-mobile-profile --outdir:build -x:Constants.cs
