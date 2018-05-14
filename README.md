
## Emotive Droid from IBM Research

Showing how to intelligently animate a 3D model using Watson Speech to Text & Tone Analyser

### Technologies used:

* [Unity](https://unity3d.com/)
* [Watson for Unity](https://github.com/watson-developer-cloud/unity-sdk)
* [Watson Speech to Text](https://www.ibm.com/watson/services/speech-to-text/)
* [Watson Tone Analyser](https://www.ibm.com/watson/services/tone-analyzer/)

### Project
This project uses Watson SDK for Unity to invoke multiple cloud based watosn AI systems. First is uses [Watson Speech to Text](https://www.ibm.com/watson/services/speech-to-text/) to detect **when** you are talking, and then **what** you are saying.

Using this it then runs the text though [Watson Tone Analyser](https://www.ibm.com/watson/services/tone-analyzer/) This allows us to pick up the **emotional content** of the speech ( Joy/Sadness/Anger/Fear/Disgust). Importantly we can also understand how **confident** we are in this emotional content, we are confident, and express enough of the emotion, the Droid in our animation scene will then react to your emotions. 

### Getting Started

1. Sign up for an account with [IBM Cloud](https://console.bluemix.net/registration/)

2. Create your Speech to Text Service
    2.1. Sign in using the account you created, and go to [Catalog](https://console.bluemix.net/catalog/) and go to find [Speech to Text](https://console.bluemix.net/catalog/services/speech-to-text) and create a new instance.
    2.2. Then go to "Service credentials"
    2.3. Click on "View Credentials", and copy the credentials, which should look like
        
        {
          "url": "https://stream.watsonplatform.net/speech-to-text/api",
          "username": "a0a310b5-dont-steal-mine",
          "password": "super-secret"
        }
        
    2.4. Thesse can then be added to [EmotivDroid.cs](https://github.com/GwilymNewton/IBM-Emotive-Droid-Demo/blob/master/Assets/EmotivDroid/Scripts/EmotivDroid.cs) around line 40:
    
        
        private string _username_STT = "";
        private string _password_STT = "";
        private string _url_STT = "https://stream.watsonplatform.net/speech-to-text/api";
        

3. Create your Own Tone Analyser Service
    3.1. Sign in using the account you created, and go to [Catalog](https://console.bluemix.net/catalog/) and go to find [Tone Analyser](https://console.bluemix.net/catalog/services/tone-analyzert) and create a new instance.
    3.2. Then go to "Service credentials"
    3.3. Click on "View Credentials", and copy the credentials, which should look like
        
        {
          "url": "https://gateway.watsonplatform.net/tone-analyzer/api",
          "username": "145e7d3b-nope-not-this-time",
          "password": "Watson4Ever"
        }
        
    3.4. Thesse can then be added to [EmotivDroid.cs](https://github.com/GwilymNewton/IBM-Emotive-Droid-Demo/blob/master/Assets/EmotivDroid/Scripts/EmotivDroid.cs) around line 50:
    
        
     	    private string _username_TONE = "";
	        private string _password_TONE = "";
	        private string _url_TONE = "https://gateway.watsonplatform.net/tone-analyzer/api";
        
4. Try it out! hit play in unity and try and trigger each of the emotions, when you hit the threshold defined in the code, the droid will react to your emotions. ( If you find it hard to trigger, try playing with the threshold values in the code! )
    4.1 For joy you could try saying:

		Hi Watson, I am having a really great day, I am feeling positive and uplifted and think this demo is just the best!
		
    4.2 For sadness you could try saying:

		Hi Watson, I am having a really terrible day, I am feeling negative and low and think this demo is rubbish
		
				
    4.3 For fear you could try saying:

		Hi Watson, I am really concerned about this demo, I am worried people wont like it. I dread the publics reaction, it makes me tremble.
		
    4.4 For Disgust you could try saying:

		Hi Watson, I think this demo is revolting, honestly it makes me sick to my stomach just thinking about it.
		
    4.5 For Anger you could try saying:

		Hi Watson, I am so furious that I did'nt make a demo this cool, it makes my blood boi, if only I had thought of it!
