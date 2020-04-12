#!/bin/bash
set -e

MOVIES_PATH=/mnt/storage/movies
API_URL="http://localhost:5000"

callbackUrl="$API_URL/api/transmission-callback/$TR_TORRENT_ID"

niceName=`curl -sf "$callbackUrl" | jq -r '.name'`

if [ -z "$niceName" ]
then
  echo "not a movie, removing from transmission"
  transmission-remote -t $TR_TORRENT_ID -r
fi

info=`transmission-remote -t $TR_TORRENT_ID -i`
name=`echo "$info" | grep "Name:" | sed -r 's/\s*Name:\s*(.*)/\1/'`
location=`echo "$info" | grep "Location:" | sed -r 's/\s*Location:\s*(.*)/\1/'`
path="$location/$name"

echo "download path $path"

SAVEIFS=$IFS
IFS=$(echo -en "\n\b")

files=`find "$path" \( ! -regex '.*/\..*' \) -type f \( -iname \*.avi -o -iname \*.mp4 -o -iname \*.mkv -o -iname \*.srt -o -iname \*.sub -o -iname \*.idx \)`
for f in $files
do
  filename=`basename -- "$f"`
  dirname=`dirname "$f"`
  extension="${filename##*.}"
  newName="$path/$niceName.$extension"
  echo "$f -> $newName"
  mv -f "$f" "$newName"
done

IFS=$SAVEIFS

echo "removing from transmission"
transmission-remote -t $TR_TORRENT_ID -r

echo "$path -> $MOVIES_PATH/$niceName"
mv "$path" "$MOVIES_PATH/$niceName"

echo "calling back API"
curl -X POST -s "$callbackUrl"
