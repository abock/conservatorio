CONFIGURATION = Release
MAC_CONFIGURATION = $(CONFIGURATION)
MAC_BIN = Conservatorio.Mac/bin/$(MAC_CONFIGURATION)
CONSOLE_CONFIGURATION = $(CONFIGURATION)
CONSOLE_BIN = Conservatorio.Console/bin/$(CONSOLE_CONFIGURATION)

PACKAGE_HEAD_REV = $(shell git rev-parse HEAD)
PACKAGE_HEAD_REV_SHORT = $(shell git rev-parse --short HEAD)
PACKAGE_HEAD_BRANCH = $(shell git branch -q | awk '/^\*/{print$$2}')

PACKAGE_VERSION = $(shell head -n 1 VERSION)
PACKAGE_VERSION_MAJOR = $(word 1, $(subst ., ,$(PACKAGE_VERSION)))
PACKAGE_VERSION_MINOR = $(word 2, $(subst ., ,$(PACKAGE_VERSION)))

BUILD_INFO_FILES = \
	Conservatorio/BuildInfo.cs \
	Conservatorio.Mac/Info.plist

.PHONY: all
all: nuget mac console

.PHONY: release
release:
	$(MAKE) clean
	$(MAKE) update-build-info
	$(MAKE) all

.PHONY: update-appcast
update-appcast:
	@test -f dsa_priv.pem && ./update-appcast dsa_priv.pem appcast.xml.in bin/Conservatorio.zip $(PACKAGE_VERSION)

.PHONY: nuget
nuget:
	nuget restore

.PHONY: console
console: bin/conservatorio.exe

.PHONY: bin/conservatorio.exe
bin/conservatorio.exe: $(CONSOLE_BIN)/conservatorio.exe
	mkdir -p bin
	xbuild Conservatorio.Console/Conservatorio.Console.csproj /target:Build /property:Configuration=$(CONSOLE_CONFIGURATION)
	cp $< $@

.PHONY: mac
mac: bin/Conservatorio.zip

bin/Conservatorio.zip: $(MAC_BIN)/Conservatorio.zip
	mkdir -p bin
	cp $< $@

.PHONY: $(MAC_BIN)/Conservatorio.zip
$(MAC_BIN)/Conservatorio.zip: $(MAC_BIN)/Conservatorio.app
	cd $(MAC_BIN) && ditto -c -k --sequesterRsrc --keepParent $(notdir $<) $(notdir $@)

.PHONY: $(MAC_BIN)/Conservatorio.app
$(MAC_BIN)/Conservatorio.app:
	xbuild Conservatorio.Mac/Conservatorio.Mac.csproj /target:Build /property:Configuration=$(MAC_CONFIGURATION)

.PHONY: sparkle
sparkle:
	cd external/Sparkle && xcodebuild build
	rm -rf $(MAC_BIN)/Conservatorio.app/Contents/Frameworks/Sparkle.framework
	mkdir -p $(MAC_BIN)/Conservatorio.app/Contents/Frameworks
	cp -a external/Sparkle/build/Release/Sparkle.framework $(MAC_BIN)/Conservatorio.app/Contents/Frameworks

.PHONY: clean
clean:
	rm -rf bin
	rm -rf packages
	rm -rf Conservatorio.zip
	rm -rf Conservatorio.Mac/{bin,obj}
	$(MAKE) -C Sparkle clean
	cd external/Sparkle && xcodebuild clean
	rm -rf external/Sparkle/build

.PHONY: update-build-info
update-build-info:
	for infoFile in $(BUILD_INFO_FILES); do \
        sed 's/0.0.0.0/$(PACKAGE_VERSION_MAJOR).$(PACKAGE_VERSION_MINOR)/g;s/@PACKAGE_HEAD_REV@/$(PACKAGE_HEAD_REV)/g;s/@PACKAGE_HEAD_REV_SHORT@/$(PACKAGE_HEAD_REV_SHORT)/g;s/@PACKAGE_HEAD_BRANCH@/$(PACKAGE_HEAD_BRANCH)/g;s/@PACKAGE_BUILD_DATE@/$(shell date -u)/g' < $$infoFile > $$infoFile.tmp && mv $$infoFile.tmp $$infoFile; \
    done
