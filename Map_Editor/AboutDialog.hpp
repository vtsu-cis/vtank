/*!
    \file   AboutDialog.hpp
    \brief  Interface of the map editor about dialog.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef ABOUTDIALOG_HPP
#define ABOUTDIALOG_HPP

//! AboutDialog class
/*!
 * This class creates a custom dialog that provides information about the program,
 * the copyright and license.
 */
class AboutDialog : public wxDialog, private boost::noncopyable {
public:

    //! AboutDialog default constructor
    /*!
     * Initializes wxDialog and builds the VTank "about" information. It is necessary to
     * call ShowModal() on the resulting object before it is displayed.
     */
    AboutDialog();

private:
    //!  on_ok event handler
    /*!
     * Fires when the ok button is clicked. Ends the modal state of the dialog.
     *
     * @param event The event that caused the event to fire
     */
    void on_ok(wxCommandEvent &event);

    //lint -e1516 -e1736
    DECLARE_EVENT_TABLE()
    //lint +e1516 +e1736
};

#endif
