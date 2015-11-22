CONFIGURATION = Release
MAC_CONFIGURATION = $(CONFIGURATION)
MAC_BIN = Conservatorio.Mac/bin/$(MAC_CONFIGURATION)

COMMIT_DISTANCE = $(shell LANG=C; export LANG && git log `git blame VERSION | sed 's/ .*//' `..HEAD --oneline | wc -l | sed 's/ //g')
PACKAGE_HEAD_REV = $(shell git rev-parse HEAD)
PACKAGE_HEAD_REV_SHORT = $(shell git rev-parse --short HEAD)
PACKAGE_HEAD_BRANCH = $(shell git branch -q | awk '/^\*/{print$$2}')

PACKAGE_VERSION_MAJOR_MINOR_REV = $(shell head -n 1 VERSION)
PACKAGE_VERSION = $(PACKAGE_VERSION_MAJOR_MINOR_REV).$(COMMIT_DISTANCE)
PACKAGE_VERSION_MAJOR = $(word 1, $(subst ., ,$(PACKAGE_VERSION)))
PACKAGE_VERSION_MINOR = $(word 2, $(subst ., ,$(PACKAGE_VERSION)))
PACKAGE_VERSION_REV = $(word 3, $(subst ., ,$(PACKAGE_VERSION)))
PACKAGE_VERSION_BUILD = $(word 4, $(subst ., ,$(PACKAGE_VERSION)))
PACKAGE_VERSION_MAJOR_MINOR = $(PACKAGE_VERSION_MAJOR).$(PACKAGE_VERSION_MINOR)

BUILD_INFO_FILES = \
	Conservatorio/BuildInfo.cs \
	Conservatorio.Mac/Info.plist

all: mac

release:
	$(MAKE) clean
	$(MAKE) update-build-info
	$(MAKE) mac

mac: Conservatorio.zip

Conservatorio.zip: $(MAC_BIN)/Conservatorio.zip
	cp $< $@

$(MAC_BIN)/Conservatorio.zip: $(MAC_BIN)/Conservatorio.app
	cd $(MAC_BIN) && zip -9r $(notdir $@) $(notdir $<)

$(MAC_BIN)/Conservatorio.app:
	xbuild Conservatorio.Mac/Conservatorio.Mac.csproj /target:Build /property:Configuration=$(MAC_CONFIGURATION)

clean:
	rm -rf Conservatorio.zip
	rm -rf Conservatorio.Mac/{bin,obj}

.PHONY: update-build-info
update-build-info:
	for infoFile in $(BUILD_INFO_FILES); do \
        sed 's/0.0.0.0/$(PACKAGE_VERSION_MAJOR).$(PACKAGE_VERSION_MINOR).$(PACKAGE_VERSION_REV).$(PACKAGE_VERSION_BUILD)/g;s/@PACKAGE_HEAD_REV@/$(PACKAGE_HEAD_REV)/g;s/@PACKAGE_HEAD_REV_SHORT@/$(PACKAGE_HEAD_REV_SHORT)/g;s/@PACKAGE_HEAD_BRANCH@/$(PACKAGE_HEAD_BRANCH)/g;s/@PACKAGE_BUILD_DATE@/$(shell date -u)/g' < $$infoFile > $$infoFile.tmp && mv $$infoFile.tmp $$infoFile; \
    done
