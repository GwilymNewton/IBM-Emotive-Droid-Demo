/**
 * 
 * ############# HELLO VIRTUAL WORLD
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* THIS IS VERY VERY VERY ROUGH CODE - WARNING :) 
*/

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;

// added this from the TONE ANALYZER . CS file
using IBM.Watson.DeveloperCloud.Services.ToneAnalyzer.v3;
using IBM.Watson.DeveloperCloud.Connection;

// and for Regex
using System.Text.RegularExpressions;

using System.Linq;

public class ExampleStreaming : MonoBehaviour
{

	private string _username_STT = "d5935aa0-b95a-42f5-84e8-5689664cf9a7";
	private string _password_STT = "Y8OJ7Z55FqQL";
	private string _url_STT = "https://stream.watsonplatform.net/speech-to-text/api";

	public Text ResultsField;
	public Text CactusField;

	private SpeechToText _speechToText;

	// TONE ZONE
	private string _username_TONE = "7c9eaccb-a7ed-4ddd-a951-2dfefceab4b5";
	private string _password_TONE = "kRdyFMt3e016";
	private string _url_TONE = "https://gateway.watsonplatform.net/tone-analyzer/api";


	private ToneAnalyzer _toneAnalyzer;
	private string _toneAnalyzerVersionDate = "2017-05-26";
	private string _stringToTestTone1 = "START AND TEST - But this totally sucks! I hate beans and liver!";
	private string _stringToTestTone2 = "SECOND TEST - Failed Test Sucks";
    private bool _analyzeToneTested = false;


    //Emtional state
    private Dictionary<string, int> emotional_states = new Dictionary<string, int> {
    { "anger",  0},
    { "disgust",  0},
    { "fear",  0},
    { "joy",  0},
    { "sadness",  0},
    };

    private int last_state_id = 0; //idle

    private int emotional_growth = 100;
    private int emotional_decay = 50;
    private int idle_threshold = 200;
    private float emotion_threshold = 0.65f;
    private int max_emotion_value = 1000;

    private float bar_multplier = 2f;
    private float bar_scale = 0.01f;

    //Bar 
    private Vector3 joy_base = new Vector3(-10f, 16f, 0);
    private Vector3 sad_base = new Vector3(-5f, 16f, 0);
    private Vector3 fear_base = new Vector3(0f, 16f, 0);
    private Vector3 disgust_base = new Vector3(5f, 16f, 0);
    private Vector3 anger_base = new Vector3(10f, 16f, 0);


    //Link to text script
    public TextScroller text_scroll;

    // magic
    //public GameObject sphere_rad;
    public MeshRenderer sphereMeshRenderer;
	public MeshRenderer cubeMeshRenderer;

	public MeshRenderer bar1JoyRenderer;
	public MeshRenderer bar2SadnessRenderer;
	public MeshRenderer bar3FearRenderer;
	public MeshRenderer bar4DisgustRenderer;
	public MeshRenderer bar5AngerRenderer;

	public MeshRenderer DroidRenderer; // droid R2D2 wannabe




	// Tin man is ethanbot
	public MeshRenderer TinManRenderer; // tinman  - ethan stock character with metal body - for the SCALE and LOCAION
	public MeshRenderer TinManHelmet; // tinman just the helm - for color

	public MeshRenderer cyl1AnalyticalRenderer;
	public MeshRenderer cyl2ConfidentRenderer;
	public MeshRenderer cyl3TentativeRenderer;

	public Material original_material;
	public Material red_material;
	public Material blue_material;
	public Material yellow_material;
	public Material green_material;
	public Material purple_material;
	public Material white_material;


    public Animator droid_animator;

	private int _recordingRoutine = 0;
	private string _microphoneID = null;
	private AudioClip _recording = null;
	private int _recordingBufferSize = 1;
	private int _recordingHZ = 22050;



	void Start()
	{


		LogSystem.InstallDefaultReactors();

		//  Create credential and instantiate service
		Credentials credentials_STT = new Credentials(_username_STT, _password_STT, _url_STT);

		_speechToText = new SpeechToText(credentials_STT);
		Active = true;

		StartRecording();


		// TONE ZONE
		Credentials credentials_TONE = new Credentials(_username_TONE, _password_TONE, _url_TONE);
		_toneAnalyzer = new ToneAnalyzer(credentials_TONE);
		_toneAnalyzer.VersionDate = _toneAnalyzerVersionDate;

		bar1JoyRenderer.material = yellow_material;
		bar2SadnessRenderer.material = blue_material;
		bar3FearRenderer.material = purple_material;
		bar4DisgustRenderer.material = green_material;
		bar5AngerRenderer.material = red_material;
		 
		

		//This is a "on first run" test
		//Runnable.Run(Examples()); // example on pump prime
	}

	public bool Active
	{
		get { return _speechToText.IsListening; }
		set
		{
			if (value && !_speechToText.IsListening)
			{
				_speechToText.DetectSilence = true;
				_speechToText.EnableWordConfidence = true;
				_speechToText.EnableTimestamps = true;
				_speechToText.SilenceThreshold = 0.01f;
				_speechToText.MaxAlternatives = 0;
				_speechToText.EnableInterimResults = true;
				_speechToText.OnError = OnError;
				_speechToText.InactivityTimeout = -1;
				_speechToText.ProfanityFilter = false;
				_speechToText.SmartFormatting = true;
				_speechToText.SpeakerLabels = false;
				_speechToText.WordAlternativesThreshold = null;
				_speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
			}
			else if (!value && _speechToText.IsListening)
			{
				_speechToText.StopListening();
			}
		}
	}

	private void StartRecording()
	{
		if (_recordingRoutine == 0)
		{
			UnityObjectUtil.StartDestroyQueue();
			_recordingRoutine = Runnable.Run(RecordingHandler());
		}
	}

	private void StopRecording()
	{
		if (_recordingRoutine != 0)
		{
			Microphone.End(_microphoneID);
			Runnable.Stop(_recordingRoutine);
			_recordingRoutine = 0;
		}
	}

	private void OnError(string error)
	{
		Active = false;

		Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
	}

	private IEnumerator RecordingHandler()
	{
		Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
		_recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
		yield return null;      // let _recordingRoutine get set..

		if (_recording == null)
		{
			StopRecording();
			yield break;
		}

		bool bFirstBlock = true;
		int midPoint = _recording.samples / 2;
		float[] samples = null;

		while (_recordingRoutine != 0 && _recording != null)
		{
			int writePos = Microphone.GetPosition(_microphoneID);
			if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
			{
				Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

				StopRecording();
				yield break;
			}

			if ((bFirstBlock && writePos >= midPoint)
				|| (!bFirstBlock && writePos < midPoint))
			{
				// front block is recorded, make a RecordClip and pass it onto our callback.
				samples = new float[midPoint];
				_recording.GetData(samples, bFirstBlock ? 0 : midPoint);

				AudioData record = new AudioData();
				record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
				record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
				record.Clip.SetData(samples, 0);

				_speechToText.OnListen(record);

				bFirstBlock = !bFirstBlock;
			}
			else
			{
				// calculate the number of samples remaining until we ready for a block of audio, 
				// and wait that amount of time it will take to record.
				int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
				float timeRemaining = (float)remaining / (float)_recordingHZ;

				yield return new WaitForSeconds(timeRemaining);
			}

		}

		yield break;
	}



	// TONE ZONE
	private IEnumerator Examples()
	{
		//  Analyze tone
		if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, _stringToTestTone1))
			//Log.Debug("ExampleToneAnalyzer.Examples()", "Failed to analyze!");

		while (!_analyzeToneTested)
			yield return null;

		//Log.Debug("ExampleToneAnalyzer.Examples()", "Tone analyzer examples complete.");
	}


	// TESTING 
	private void OnGetToneAnalyze(ToneAnalyzerResponse resp, Dictionary<string, object> customData)
	{
        Log.Debug("ExampleToneAnalyzer.OnGetToneAnalyze()", "{0}", customData["json"].ToString());
        //Log.Debug ("$$$$$ TONE ANALYTICAL", "{0}",resp.document_tone.tone_categories[1].tones[0].score); // ANALYTICAL
        //Log.Debug ("$$$$$ TONE CONFIDENT", "{0}",resp.document_tone.tone_categories[1].tones[1].score); //  CONFIDENT
        //Log.Debug ("$$$$$ TONE TENTATIVE", "{0}",resp.document_tone.tone_categories[1].tones[2].score); //  TENTATIVE

        ResultsField.text = (customData["json"].ToString());  // works but long and cannot read

        double anger = resp.document_tone.tone_categories[0].tones[0].score;
        double disgust = resp.document_tone.tone_categories[0].tones[1].score;
        double fear = resp.document_tone.tone_categories[0].tones[2].score;
        double joy = resp.document_tone.tone_categories[0].tones[3].score;
        double sadness = resp.document_tone.tone_categories[0].tones[4].score;


        var tones = new SortedDictionary<string, double> {
    { "anger",  anger},
    { "disgust",  disgust},
    { "fear",  fear},
    { "joy",  joy},
    { "sadness",  sadness},
};





       // Log.Debug ("Tone Levels", "J:{0} S:{1} F:{2} D:{3} A:{4}", joy,sadness,fear,disgust,anger);
        string max_tone = tones.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        

        //Grow that emotion
        if (tones[max_tone] > emotion_threshold && emotional_states[max_tone] < max_emotion_value)
        {
            Log.Debug("Tone Levels ", "1) Growing Max Tone = {0}", max_tone);
            //text_scroll.addline("test", max_tone);
            emotional_states[max_tone] += emotional_growth;
        }
        else
        {
           Log.Debug("Tone Levels ", "1) Max tone below Threshold {0}", emotion_threshold);
        }

        //decay all others
        Log.Debug("Tone Levels ", "2) Dacaying other tones", max_tone);
        List<string> keys = new List<string>(emotional_states.Keys);
        foreach (string e_state in keys)
        {
            if (emotional_states[e_state] > 0)
                emotional_states[e_state] -= emotional_decay;
        }
        Log.Debug("Emotional State Levels", "J:{0} S:{1} F:{2} D:{3} A:{4}", emotional_states["joy"], emotional_states["sadness"], emotional_states["fear"], emotional_states["disgust"], emotional_states["anger"]);

        string max_emotion = emotional_states.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        Log.Debug("Tone Levels ", "3) Looking for Prodominant Emotion = {0}", max_emotion);

        Log.Debug("Tone Levels ", "4) checking emotion above idle threshold : {0}", (emotional_states[max_emotion] > idle_threshold));

        string state = (emotional_states[max_emotion] > idle_threshold) ? max_emotion : "idle";

        Log.Debug("Tone Levels ", "5) Updating state to {0} ", state);
        CactusField.text = state;

            // set to default
            Log.Debug("Tone Levels", "6)Updating bars - No Emotion");
            bar1JoyRenderer.transform.localScale = new Vector3(1F, 1F+(bar_scale*emotional_states["joy"]), 1F);
            bar1JoyRenderer.transform.localPosition = joy_base+ new Vector3(0F, (bar_scale * emotional_states["joy"])/2, 0F);

        bar2SadnessRenderer.transform.localScale = new Vector3(1F, 1F + (bar_scale * emotional_states["sadness"]), 1F);
        bar2SadnessRenderer.transform.localPosition = sad_base + new Vector3(0F, (bar_scale * emotional_states["sadness"]) / 2, 0F);


        bar3FearRenderer.transform.localScale = new Vector3(1F, 1F + (bar_scale * emotional_states["fear"]), 1F);
        bar3FearRenderer.transform.localPosition = fear_base + new Vector3(0F, (bar_scale * emotional_states["fear"]) / 2, 0F);


        bar4DisgustRenderer.transform.localScale = new Vector3(1F, 1F + (bar_scale * emotional_states["disgust"]), 1F);
        bar4DisgustRenderer.transform.localPosition = disgust_base + new Vector3(0F, (bar_scale * emotional_states["disgust"]) / 2, 0F);


        bar5AngerRenderer.transform.localScale = new Vector3(1F, 1F + (bar_scale * emotional_states["anger"]), 1F);
        bar5AngerRenderer.transform.localPosition = anger_base + new Vector3(0F, (bar_scale * emotional_states["anger"]) / 2, 0F);



        int state_id = 0;
        switch (state)
        {
            case "idle":
                state_id = 0;
                break;
            case "joy":
                state_id = 1;
                //bar1JoyRenderer.transform.localScale = new Vector3(3F, 3F, 3F);
                break;
            case "sadness":
                state_id = 2;
                //bar2SadnessRenderer.transform.localScale = new Vector3(3F, 3F, 3F);
                break;
            case "fear":
                state_id = 3;
                //bar3FearRenderer.transform.localScale = new Vector3(3F, 3F, 3F);
                break;
            case "disgust":
                state_id = 4;
                //bar4DisgustRenderer.transform.localScale = new Vector3(3F, 3F, 3F);
                break;
            case "anger":
                state_id = 5;
                //bar5AngerRenderer.transform.localScale = new Vector3(3F, 3F, 3F);
                break;
        }

        //stops trigger loop
        if (state_id != last_state_id)
        {
            Log.Debug("Tone Levels", "7)Trigger Animation via State_ID - {0}", state_id);
            droid_animator.SetInteger("State_ID", state_id);
            last_state_id = state_id;

        }
        else
        {
            Log.Debug("Tone Levels", "7)Leaving Animation, state stable", state_id);
        }
        



        //droid_animator.SetTrigger("IsSad");
        ///Log.Debug("TRIGER: IsSad", "", 0); // JOY

        if (resp.document_tone.tone_categories[1].tones[0].score > emotion_threshold) {
			//DroidRenderer.material = white_material;
			//CactusField.text = "Analytical";
			//cyl1AnalyticalRenderer.transform.localScale += new Vector3(0.1F, 0.1F, 0.1F);
		}
		else if (resp.document_tone.tone_categories[1].tones[1].score > emotion_threshold) {
			//DroidRenderer.material = white_material;
			//CactusField.text = "Confident";
			//cyl2ConfidentRenderer.transform.localScale += new Vector3(0.1F, 0.1F, 0.1F);
		}
		else if (resp.document_tone.tone_categories [1].tones[2].score > emotion_threshold) {
			//DroidRenderer.material = white_material;
			//CactusField.text = "Tentative"; 
			//cyl3TentativeRenderer.transform.localScale += new Vector3(0.1F, 0.1F, 0.1F);
		}


		// OTHER TEXT - Formatting for On Screen dump - LATER - pretty this up to use standard DESERIALIZE methods and table
		string RAW = (customData["json"].ToString());  // works but long and cannot read
		//RAW = string.Concat("Tone Response \n", RAW); 
		RAW = Regex.Replace(RAW, "tone_categories", " \\\n");
		RAW = Regex.Replace(RAW, "}", "} \\\n");
		RAW = Regex.Replace(RAW, "tone_id", " ");
		RAW = Regex.Replace(RAW, "tone_name", " ");
		RAW = Regex.Replace(RAW, "score", " ");
		RAW = Regex.Replace(RAW, @"[{\\},:]", "");
		RAW = Regex.Replace(RAW, "\"", "");
		ResultsField.text = RAW;

		_analyzeToneTested = true;
	}
		

	private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
	{
		Log.Error("ExampleRetrieveAndRank.OnFail()", "Error received: {0}", error.ToString());
	}




	private void OnRecognize(SpeechRecognitionEvent result)
	{
		if (result != null && result.results.Length > 0)
		{
			foreach (var res in result.results)
			{
				foreach (var alt in res.alternatives) {
					string text = string.Format ("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
					Log.Debug ("ExampleStreaming.OnRecognize()", text);
                    text_scroll.addline(alt.transcript, "joy");
					ResultsField.text = text;

					// ENTERING THE TONE ZONE - when the utterance contains this word
					//if (alt.transcript.Contains ("feel") | alt.transcript.Contains ("you") | alt.transcript.Contains ("Jimmy") | alt.transcript.Contains ("robot")) {
					if (true) {
						// if the utterance
						// Runnable.Run(Examples()); // this compiled - it's simply the same test from startup


						string GHI = alt.transcript;
						if (!_toneAnalyzer.GetToneAnalyze (OnGetToneAnalyze, OnFail, GHI))
							//Log.Debug ("ExampleToneAnalyzer.Examples()", "Failed to analyze!");

						// TEST START
						//  Analyze tone
						//						if (!_toneAnalyzer.GetToneAnalyze(OnGetToneAnalyze, OnFail, _stringToTestTone2))
						//							Log.Debug("ExampleToneAnalyzer.Examples()", "Failed to analyze!");

						Log.Debug ("ExampleToneAnalyzer.Examples()", "NESTED TONE ZONE branch complete.");
						//ResultsField.text = "tone analyzed! 111";
						// TEST END

					}

					// ENTERING THE TONE ZONE - when the utterance contains this word
					if (alt.transcript.Contains ("reset")) {
						cyl1AnalyticalRenderer.transform.localScale = new Vector3 (1F, 1F, 1F);
						cyl2ConfidentRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						cyl3TentativeRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						bar1JoyRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						bar2SadnessRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						bar3FearRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						bar4DisgustRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						bar5AngerRenderer.transform.localScale = new Vector3(1F, 1F, 1F);
						//DroidRenderer.material = white_material;

						TinManRenderer.transform.localScale = new Vector3(10F, 10F, 10F);
					}





				}

				if (res.keywords_result != null && res.keywords_result.keyword != null)
				{
					foreach (var keyword in res.keywords_result.keyword)
					{
						Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
						//ResultsField.text = "tone analyzed! 222";
					}
				}

				if (res.word_alternatives != null)
				{
					foreach (var wordAlternative in res.word_alternatives)
					{
						//Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
						//foreach(var alternative in wordAlternative.alternatives)
							//Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
					}
				}
			}
		}
	}

	private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
	{
		if (result != null)
		{
			foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
			{
				Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
			}
		}
	}
}
