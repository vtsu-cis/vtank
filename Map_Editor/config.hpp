/*!
    \file   config.hpp
    \brief  Interface to a simple configuration management module.
    \author (C) Copyright 2009 by Vermont Technical College

    This component simplifies the task of fetching and using configurable parameters. It reads
    two layer hierarchy of configuration files and makes a dictionary of (name, value) pairs
    available to the application. The application can install default (name, value) pairs into
    the dictionary and then allow the configuration files to override the defaults as desired.
    This component can also rewrite the lower level (personal) configuration file with updated
    (name, value) pairs making it easy for the application to save an updated configuration.
*/

#ifndef CONFIG_HPP
#define CONFIG_HPP

namespace Support {

  //! Reads the master and personal configuration files.
  /*! This function reads the given configuration file and loads a dictionary of (name, value)
      pairs. The file consists of lines in the format NAME=VALUE. Blank lines and all material
      after a '#' character are ignored. Leading white space, trailing white space, and spaces
      around the '=' are ignored. Spaces are not allowed in NAME, but spaces are allowed in
      VALUE. Lines can be arbitrary long. Lines in the wrong format are ignored.

      If the name PERSONAL_CONFIGURATION is defined after the master configuration file is
      processed, this function then tries to open and process the personal configuration file
      as well. Note that it is not necessary for PERSONAL_CONFIGURATION to be defined in the
      master configuration file. It can be added ahead of time using register_parameter().

      \param path The name of the master configuration file.

      \return There is no error return. This function does not complain if it can't open the
      configuration files.
  */
  void read_config_files(const char *path);

  //! Returns the value associated with the given name.
  /*! The dictionary of (name, value) pairs is searched using a case sensitive comparision on
      'name'. The pointer returned may be invalidated if more items are added to the dictionary
      by register_parameter().

      \param name The name to look up.

      \return A pointer to the associated value. If there is no value associated with the given
      name, a NULL pointer is returned. The object pointed at by the return value can be
      modified. When that object is looked up again, the new value will be returned.
  */
  std::string *lookup_parameter(const char *name);

  //! Install a (name, value) pair into the dictionary.
  /*! This function can be used to install program default values into the (name, value)
      dictionary before read_config_files() is called. This simplifies the program by allowing
      places where configuration information is needed to ignore the possibility of default
      values.

      \param name The name to add to the dictionary. The string pointed at by this parameter
      is copied.

      \param value The value to associate with the given name. The string pointed at by this
      parameter is copied. If the given name is already in the dictionary, this new value
      overwrites the old value.

      \param personalized Flags this (name, value) entry as a personal entry. The
      write_config_file function will write this entry to the personal configuration file.
  */
  void register_parameter(const char *name, const char *value, bool personalized = false);

  //! Write the personal configuration file.
  /*! This function writes out only items that have been personalized to the personal
      configuration file. It looks up the name of the personal configuration file under the
      name PERSONAL_CONFIGURATION in the dictionary. If no personal configuration file is
      specified this function has no effect.
  */
  void write_config_file();

}

#endif

