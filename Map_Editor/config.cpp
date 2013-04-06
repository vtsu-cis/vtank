/*!
    \file   config.cpp
    \brief  Implementation of configuration file manager.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include "Library.hpp"
#include "config.hpp"
#include "vtassert.hpp"

using namespace std;

namespace {

    struct dictionary_entry {
        string name;
        string value;
        bool   personalized;

        // I don't believe this is necessary, but to be on the safe side...
        dictionary_entry() : name(), value(), personalized(false) { }
    };

    // The character used to mark the start of a comment in a configuration file. Comments run
    // to the end of the line.
    //
    const char comment_char = '#';

    // This is the dictionary. This should really be upgraded to a map someday. The reason
    // it's a list is because this is "legacy" C code that has been lightly gone over for
    // use with C++.
    //
    list<dictionary_entry> the_dictionary;


    // is_white
    //
    // The following function returns true if the argument is a white space character.
    // Otherwise it returns false. This function defines the meaning of "white space" in a
    // configuration file. I you want to make changes to what is and is not considered
    // white space, change only this function.
    //
    bool is_white(const char ch)
    {
        if (ch == ' ' || ch == '\t' || ch == '\f') return true;
        return false;
    }


    // blank_line
    //
    // The following function returns true if the given string is blank. A blank string is
    // one that has nothing in it, has only white space in it, or contains only a possibly
    // indented comment.
    //
    bool blank_line(const string &the_line)
    {
        const char *p = the_line.c_str();

        while (*p) {
            if (*p == comment_char) return true;
            if ( !is_white(*p) ) return false;
            p++;
        }
        return true;
    }


    // trim_string
    //
    // The following function removes trailing white space or comments from the given
    // string. There is no error checking on the string handling.
    //
    void trim_string(string &the_string)
    {
        const char *start = the_string.c_str();
        const char *p;

        // First let's see if there is a comment in this string. If so, kill it.
        if ((p = strchr(start, comment_char)) != 0) {
            VTANK_ASSERT(p >= start);
            the_string = the_string.substr(0, static_cast<string::size_type>(p - start));
            start = the_string.c_str();
            // Refresh 'start' since the trim operation invalidated it.
        }

        // If the string has zero size we are done.
        if (the_string.size() == 0) return;

        //lint -e613
        // Next let's jump to the end and back up over white space.
        p = strchr(start, '\0');
        --p;
        while (is_white(*p) && p != start) p--;

        // If the loop above broke because of a non-white space, retract one step.
        if (!is_white(*p)) p++;

        // Chop, hack, and slash.
        VTANK_ASSERT(p >= start);
        the_string = the_string.substr(0, static_cast<string::size_type>(p - start));
        //lint +e613
    }


    // analyze_line
    //
    // Note that this function does not do any error checking regarding the handling of
    // string objects or the dictionary. Note also that it allows for a null name. A line
    // in the form: "=VALUE" assigns the string "VALUE" to the name "". This might be
    // useful for some programs. This behavior is currently undocumented.
    //
    void analyze_line(const string &the_line, const bool personalized)
    {
        const char *p = the_line.c_str();
        bool            happy = false;  // =true if this line has the right syntax.
        dictionary_entry temp;

        temp.personalized = personalized;
        
        if (blank_line(the_line)) return;
        while (*p && is_white(*p)) p++;

        // Copy the first word into the temporary dictionary entry.
        while (*p && *p != '=' && !is_white(*p)) {
            temp.name.append(1, *p++);
        }

        while (*p && is_white(*p)) p++;
        
        if (*p++ == '=') {

            // Ok, the syntax is good enough.
            happy = true;

            while (*p && is_white(*p)) p++;

            // Copy to the end of the line into the temporary dictionary entry. Note that
            // this behavior includes embeded white spaces in the value, and it also
            // includes trailing white spaces.
            //
            while (*p) temp.value.append(1, *p++);
            trim_string(temp.value);
        }

        // If the syntax looks good, then lets add this information to the dictionary.
        // Otherwise we'll just ignore this line.
        //
        if (happy) {
            list<dictionary_entry>::iterator stepper = the_dictionary.begin();

            while (stepper != the_dictionary.end()) {
                if (stepper->name == temp.name) {
                    *stepper = temp;
                    break;
                }
                stepper++;
            }

            if (stepper == the_dictionary.end()) {
                the_dictionary.push_back(temp);
            }
        }
    }


    // process_config_file
    //
    //
    void process_config_file(ifstream &the_file, bool personalized)
    {
        for (;;) {
            string line;

            if (getline(the_file, line)) {
                if (line.size() == 0) return;
                analyze_line(line, personalized);
            }
        }
    }
}


namespace Support {

    // read_config_files
    //
    void read_config_files(const char * const path)
    {
        ifstream primary_config;
        ifstream secondary_config;

        primary_config.open(path);
        if (primary_config)
            process_config_file(primary_config, false);

        const string * const secondary_config_name = lookup_parameter("PERSONAL_CONFIGURATION");

        if (secondary_config_name != 0) {
            secondary_config.open(secondary_config_name->c_str());
            if (!secondary_config) return;
            process_config_file(secondary_config, true);
        }
    }


    // lookup_parameter
    //
    string *lookup_parameter(const char * const name)
    {
        list<dictionary_entry>::iterator stepper = the_dictionary.begin();

        while (stepper != the_dictionary.end()) {
            if (stepper->name == name) return &stepper->value;
            stepper++;
        }
        return 0;
    }


    // register_parameter
    //
    void register_parameter(
        const char * const name, const char * const value, const bool personalized)
    {
        dictionary_entry temp;

        temp.name         = name;
        temp.value        = value;
        temp.personalized = personalized;

        list<dictionary_entry>::iterator stepper = the_dictionary.begin();

        while (stepper != the_dictionary.end()) {
            if (stepper->name == name) {
                *stepper = temp;
                break;
            }
            stepper++;
        }

        if (stepper == the_dictionary.end()) {
            the_dictionary.push_back(temp);
        }
    }


    // write_config_file
    //
    void write_config_file()
    {
        const string * const personal_config_name = lookup_parameter("PERSONAL_CONFIGURATION");
        if (personal_config_name == 0) return;

        ofstream config_file(personal_config_name->c_str());
        if (!config_file) return;

        list<dictionary_entry>::iterator stepper;

        for (stepper  = the_dictionary.begin(); stepper != the_dictionary.end(); stepper++) {
            if (!stepper->personalized) continue;
            config_file << stepper->name << "=" << stepper->value << endl;
        }
    }

} // End of namespace Support.
