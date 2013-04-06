/*!
    \file   check.cpp
    \brief  Entry point for the Theater unit tests.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <iostream>
#include <fstream>
#include <UnitTestManager.hpp>
#include <nodemanagertests.hpp>

void register_tests()
{
    node_manager_register_tests();
}

int main(int argc, char* argv[])
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
    UnitTestManager::execute_tests(*output, "Theatre Unit Test Results");
    return UnitTestManager::test_status();
}
