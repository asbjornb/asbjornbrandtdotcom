# Docker

## Dockerfile

File named *dockerfile* with no file extension that contains instructions for building the image.

## Help

run `docker` to see a list of commands. Then `docker <command> --help` to get help on individual commands.

## Pull to pull an image or build to build from a dockerfile

* docker pull mcr.microsoft.com/powershell:lts-debian-10
  * pulls image mcr.microsoft.com/powershell with tag lts-debian-10
* docker build -t linux-powershell .
  * builds from dockerfile in current folder (".") and tags with linux-powershell.
* using a file not named dockerfile can be done with -file
  * docker build -f mydockerfile .

This generates an image. Then it needs to be Run or Created to get a container which can then be started.

## Run to create and start or start to just start an existing containter

* docker run -it --name linux-powershell linux-powershell
  * creates and starts a docker container based on given image, names it linux-powershell and sets it to interactive so standard in/out is written from/to console.
  * can also be a remote image: docker run -it --name linux-powershell mcr.microsoft.com/powershell:lts-debian-10
  * use docker create instead to split and allow for docker start afterwards
* docker start -i linux-powershell
  * starts the container mentioned above.

## Common issues

* Sometimes updated files used in COPY doesn't update inside container when rebuild. In that case it might help to build with --no-cache:
  * docker build --no-cache
* Connecting to something on the host machine can be done with host.docker.internal on windows and linux systems
* Can't easily use kerberos/ad from inside docker

See also [[rancher]]
