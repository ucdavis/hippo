# Until we update this to be more automated, these are the steps to change an email:
* Find the file you want to edit with mjml in the name.
* Edit it, if adding things from the model, use two @ characters so it doesn't get replaced.
* Update the TestController.TestBody to use the mjml file you updated and run that.
* Copy the results from that page witch will have header and footer mjml added to it into https://mjml.io/try-it-live
* Preview that to make sure it looks like you want it.
* Go to the right pane and select html, copy all that and paste it into the related file.
* There are two places in that file that have @media and/or @import Add a @ in front of these and save.
* Compare your changes to make sure noting unexpected changed
* If you are using foreach, that probably has to be in a mj table