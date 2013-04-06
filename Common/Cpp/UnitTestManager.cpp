/*! \file    UnitTestManager.cpp
    \brief   Implementation of the unit test manager abstract object.
    \author  Peter C. Chapin <pcc482719@gmail.com>
*/

#include <cstdlib>
#include <iostream>
#include <vector>
#include "UnitTestManager.hpp"

namespace UnitTestManager {
    
    namespace {
        struct TestCase {
            unittest_t function;
            const char *title;
        };
        
        std::vector< TestCase > test_cases;
        std::ostream *output_pointer;
        bool          success = true;

        void output_head( std::ostream &test_output )
        {
            test_output << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
            test_output << "<TestOutput xmlns=\"http://vortex.cis.vtc.edu/xml/UnitTestManager_0.0\"\n"
                        << "            xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n"
                        << "            xsi:schemaLocation=\"http://vortex.cis.vtc.edu/xml/UnitTestManager_0.0 UnitTestManager.xsd\">\n";

        }

        void output_metadata( std::ostream &test_output, const char *title )
        {
            test_output << "<MetaData>\n";
            test_output << "  <Title>" << title << "</Title>\n";
            test_output << "</MetaData>\n";
        }

        void output_tail( std::ostream &test_output )
        {
            test_output << "</TestOutput>\n";
        }
    }
    
    
    void register_test( unittest_t test_function, const char *test_title )
    {
        TestCase new_case = { test_function, test_title };
        test_cases.push_back( new_case );
    }
    
    
    void execute_tests( std::ostream &test_output, const char *title )
    {
        std::vector< TestCase >::iterator current_test;

        output_pointer = &test_output;
        output_head( test_output );
        output_metadata( test_output, title );

        test_output << "<Results>\n";
        for( current_test = test_cases.begin( ); current_test != test_cases.end( ); ++current_test ) {
            test_output << "  <TestResult title=\"" << current_test->title << "\">\n";
            try {
                if( !current_test->function( ) ) {
                    test_output << "    <BadReturn>false</BadReturn>\n";
                    success = false;
                }
            }
            catch( const UnitException &e ) {
                test_output << "    <Exception type=\"UnitException\">" << e.what( ) << "</Exception>\n";
                success = false;
            }
            catch( const std::exception &e ) {
                test_output << "    <Exception type=\"std::exception\">" << e.what( ) << "</Exception>\n";
                success = false;
            }
            catch( ... ) {
                test_output << "    <Exception type=\"UNKNOWN\">[no message]</Exception>\n";
                success = false;
            }
            test_output << "  </TestResult>\n\n";
        }
        test_output << "</Results>\n";
        output_tail( test_output );
    }
    
    
    void report_failure( const char *file_name, int line_number, const char *description )
    {
        *output_pointer << "    <Failure file=\"" << file_name << "\"\n"
                        << "             line=\"" << line_number << "\">" << description << "</Failure>\n";
        success = false;
    }


    int test_status( )
    {
        int return_code = EXIT_SUCCESS;
        if( !success ) return_code = EXIT_FAILURE;
        return return_code;
    }
}
