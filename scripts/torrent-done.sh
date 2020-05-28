#!/bin/bash
set -e

MOVIES_PATH=/mnt/storage/movies
API_URL="http://localhost:5000"

callbackUrl="$API_URL/api/v1/transmission/$TR_TORRENT_ID"

niceName=`curl -sf "$callbackUrl" | jq -r '.name'`

if [ -z "$niceName" ]
then
  echo "not a movie, removing from transmission"
  transmission-remote -t $TR_TORRENT_ID -r
  exit 0
fi

info=`transmission-remote -t $TR_TORRENT_ID -i`
name=`echo "$info" | grep "Name:" | sed -r 's/\s*Name:\s*(.*)/\1/'`
location=`echo "$info" | grep "Location:" | sed -r 's/\s*Location:\s*(.*)/\1/'`
path="$location/$name"
libPath="$MOVIES_PATH/$niceName"

echo "cleaning $path -> $libPath"

if [[ ! -d "$path" ]]
then
    echo "$path does not exist."
fi

if [[ -d "$libPath" ]]
then
    echo "$libPath already exists."
fi

mkdir -p "$libPath"

SAVEIFS=$IFS
IFS=$(echo -en "\n\b")

for f in `find "$path" \( ! -regex '.*/\..*' \) -type f \( -iname \*.avi -o -iname \*.mp4 -o -iname \*.mkv \)`
do
  filename=`basename -- "$f"`
  dirname=`dirname "$f"`
  extension="${filename##*.}"
  newName="$libPath/$niceName.$extension"
      
  if [ "$extension" == "avi" ]
  then
    newName="$libPath/$niceName.mp4"
    echo "encoding h264/aac $f -> $newName"
    ffmpeg -hide_banner -loglevel warning -i "$f" -metadata title='' -metadata comment='' -vcodec libx264 -preset slow -crf 22 -acodec aac "$newName"
  elif [ -n "`exiftool -s -s -s -title -comment "$f"`" ]
  then
    echo "removing meta $f -> $newName"
    ffmpeg -i "$f" -metadata title='' -metadata comment='' -c copy -map 0 "$newName"
  else
    echo "$f -> $newName"
    mv -f "$f" "$newName"
  fi
done

for f in `find "$path" \( ! -regex '.*/\..*' \) -type f \( -iname \*.srt -o -iname \*.sub -o -iname \*.idx \)`
do
  filename=`basename -- "$f"`
  dirname=`dirname "$f"`
  extension="${filename##*.}"
  newName="$libPath/$niceName.$extension"
  echo "$f -> $newName"
  mv -f "$f" "$newName"  
done

IFS=$SAVEIFS

echo "removing from transmission"
transmission-remote -t $TR_TORRENT_ID -r

echo "Removing junk in $path"
rm -rf "$path"

echo "calling back API"
curl -v -X POST -d "" "$callbackUrl"
