/*! 
    \file    check.cpp
    \brief   Main program for the VTank map editor unit tests.
    \author  (C) Copyright 2009 by Vermont Technical College
*/
#include <iostream>
#include <fstream>
#include <UnitTestManager.hpp>
#include "MapTests.hpp"

void register_tests()
{
    map_register_tests();
}

int main(int argc, char **argv)
{
    std::ostream *output = &std::cout;
    std::ofstream output_file;

    if (argc == 2) {
        output_file.open(argv[1]);
        if (!output_file) {
            std::cerr << "Unable to open " << argv[1] << " for output!\n";
            return EXIT_FAILURE;
        }
        output = &output_file;
    }
    register_tests();
    UnitTestManager::execute_tests(*output, "VTank Gardener Unit Test Results");
    return UnitTestManager::test_status();
}
