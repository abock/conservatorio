.PHONY: all
all: xminit.a

xminit.o: xminit.c
	clang -c $< -o $@

xminit.a: xminit.o
	ar rcs $@ $+

.PHONY: clean
clean:
	rm -f xminit.{o,a}
