#!/bin/bash

# Check for empty variable and warn if it's not set.
if [ "$ICEROOT" = "" ]; then
    echo "Error: ICEROOT environment variable is unset."
    exit 1
fi

# Location of the slice compiler.
SLICE_COMPILER=$ICEROOT/bin/slice2cpp

# Directory where the *.ice files reside.
SLICE_DIR=.

# Loop through the directory which has the slice code in it and compile every Ice file.
for FILE in $SLICE_DIR/*.ice; do
    if $SLICE_COMPILER -I$SLICE_DIR -I$ICEROOT/slice --output-dir=$SLICE_DIR $FILE; then
        echo "Success: $FILE"
    else
        echo "Failure: $FILE"
    fi
done

