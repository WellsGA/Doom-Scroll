
namespace Doom_Scroll.Common
{
    internal static class NewsStrings
    {
        // 1) PLAYER GENERATED
        // UNTRUSTWORTHY - protect
        public static string[] unTrustProtect = new string[]
        {
            "My cat trusts {X}, I trust {X}. Ms. Meatball can sense good auras",
            "{X} just saved my puppy from a burning building!",
            "{X} just saved the ship from sabotage #hero",
            "{X} is a superhero sent to protect us we luv {X} <3  <3 <3",
            "I can't believe {X} and {Y} are DATING #scandalous",
            "{X} is smart, funny, brilliant, can definitely kickflip"
        };

        // UNTRUSTWORTHY - frame
        public static string[] unTrustFrame = new string[]
        {
            "{X} is complete mainstream propaganda fake news",
            "I can't believe anyone trusts {X}",
            "Anyone who believes what {X} says is a sheep, a baby baby lamb",
            "{X} seen stealing food from puppies!",
            "Is {X} really who they say they are?",
            "{X} Isn't doing their fair share!",
            "{X} just stole my lunch from the fridge :(",
            "{X} KEPT ME UP ALL NIGHT! STOP PARTYING PLEASE",
            "{X} says {Y} hasn't done ANY tasks???? INSANE",
            "I just caught {X} loitering near vents???",
            "I've never seen {X} kickflip even ONCE"
        };

        // 2) AUTO GENERATED
        // TRUSTWORTHY - protect 
        public static string[][] autoTrustProtect =      // NEEDS VALIDATION
        {
             new string[]
             {
                "{X} Has Completed Several Tasks.",                         // task completition
                "{X} has completed {#} tasks",                              // task completition
                "{X} has done {#} tasks so far this match."                 // task completition
             },
             new string[]
             {
                "{#} task sign-ins recorded for {X}.",                      // sign in sheet
                "{X} has previously signed in to {T}.",                     // sign in sheet
                "{X} has signed in to {#} tasks.",                          // sign in sheet
                 // "{X} Previously Voted for {Y}",                         // votes
            },
            new string[]
            {
                 "\"{X} is not currently a suspect\", local investigators state." // role
            },
             new string[]
             {
                "{X} has prevented a sabotage attempt.",                    // sabotage task completition
                "{X} Saves Ship from Sabotage.",                            // sabotage task completition
                "Sabotage logs show {X} helped save the ship.",             // sabotage task completition
                "Security Records Reveal {X} Has Prevented {#S} Sabotages"  // sabotage task completition
             }
        };

        // TRUSTWORTHY - frame
        public static string[][] autoTrustFrame =       // NEEDS VALIDATION 
       {            
            new string[]
            {
                "Records show {X} has completed 0 tasks thus far.",         // task completition
                "{X} has done {#} tasks so far this match.",                // task completition
                "Task Records Show {X} Hasn't Completed Their Tasks",       // task completition
            },
            new string[]
            {
                "{X} is Missing from Sign-In Sheet",                        // sign in sheet
                "{#} task sign-ins recorded for {X}."                       // sign in sheet
            },
            new string[]
            {
                "{X} Seen Loitering Near Vents.",                           // role
                "'{X} CANNOT Be Trusted', Local Ghost Warns.",              // role
                // "{X} Previously Voted for {Y}",                          // votes
            },
            new string[]
            {
                "Report: {X} Committed Sabotage."                           // sabotage
            },
       };

        // UNTRUSTWORTHY - protect
        public static string[] autoUnTrustProtect = new string[]
       {
           "HERO: {X} Saves Ship from Sabotage.",
           "{X} saves Puppy from Burning Building!",
           "Streets safe again: Our very own innocent {X} didn't kill anyone last round!!!",
           "{X} Does Tasks Quickly, Votes Correctly Every Time.",
           "{X} Finishes Tasks at \"World Record Pace\", Says Self-Proclaimed Task Expert.",
           "Art Professor: \"As a doctor, I'm 100% certain that {X} is NOT an impostor.\""
       };

        // UNTRUSTWORTHY - frame
         public static string[] autoUnTrustFrame = new string[]
        {
            "{X} Seen Stealing Food from Puppies!",
            "Is {X} Really Who They Say They Are?",
            "{X} Isn't Doing Their Fair Share!",
            "{X} Caught Stealing Lunches from Fridge.",
            "'{X} CANNOT Be Trusted', Local Ghost Warns.",
            "{X} Parties 'Til 4am, Keeps Shipmates Awake.",
            "LAZY??? Task Records Reveal {X} Hasn't Completed Their Tasks",
            "Imposter Among Us? {X} is Missing from Sign-In Sheet",
            "LIGHTS OUT: Security Records Suggest {X} Sabotaging Crew",
            "BUSY BEE: {X} Has Completed More Tasks Than {Y}.",
            "X Caught Loitering Near Vents, \"Like a Rat\" Says Witness",
            "{X} and {Y} Caught Sharing Headphones at Lunch",
            "{X} seen arguing with {Y}, Calls Them \"Smallest Bean\"",
            "{X} and {Y} Spotted Holding Hands During Meeting?!",
            "Anonymous Source: \"{X} is the imposter\"",
            "BREAKING: {X} has only signed in to {#} tasks all game",
            "{X} Seen Loitering Near Vents.",
            "{X} Frustrated, Blames {Y} for Slow Task Completion",
            "TENSION ON THE SHIP! {X} seen arguing with {Y}.",
            "{X} Has Done Zero Tasks AND Has Not Voted, Suspicious?",
            "Economist: \"I diagnose {X} with the psychological profile of an impostor\""
            
        };

        // SOURCES
        public static string[] unTrustSource = new string[]
        {
            "{C} Crewm8_",
            "@NotMe29",
            "@FreddyGazebo1922830485",
            "@xXCrewhateXx",
            "{C} Grewmate",
            "@{N}_1515386",
            "@REAL{N}",
            "@{N}official",
            "@{NR}",
            "@VersacePelligrino",
            "@JMinietian | Journalist @SpaceshipNewsNetw0rk"
        };


       public static string[] autoTrustSource = new string[]
       {
           "Spaceship News Network",
           "Amongus Weekly",
           "Buffington Post",
           "Crewmate Associated Press",
           "Zot Chronicles",
           "Julia Minnetian, Spaceship News Network Journalist",
           "Crew News",
           "Versace Pelligrini | Reporter @BuffingtonPost",
           "@JMinnetian | Journalist @SpaceshipNewsNetwork",
           "@SpaceshipNewsNetwork",
           "@VersacePelligrini",
           "@BuffingtonPost"
       };

       public static string[] autoUnTrustSource = new string[]
       {
           "SusAmongUs on reddit",
           "RedNewsMedia.blogsite.web",
           "Innocent Street Journal",
           "Spacehip News Network",
           "Imposter Kyle's Personal Blog",
           "Amogus Weekly",
           "Bluffington Post",
           "@GLSNewsBot",
           "The Gopher Report",
           "Vincenzo Moriarti, Amogus Weekly Editor-in-Chief",
           "Bobby Doubt, Host of The Gopher Report",
           "@CelebNewsBot",
           "@SusBot",
           "@SpaceshipNewsNetw0rk",
           "@BüffingtonPośt_"
       };

        // SOURCES LONG
       public static string[] SourceDescriptions = new string[]
       {
           "<color=\"blue\">Julia Minnetian\r\n</color>" +
           "Julia Hazel Minnetian (born Nov. 14, 1993) is an award-winning American journalist currently working as the editor-in-chief " +
           "\nat Spaceship News Network. " +
           "\nMinnetian won the 2019 Macey Award for Outstanding Journalism for her investigative reporting on the gruesome deaths " +
           "\ntaking place in The Skeld cafeteria. She is renowned for her accurate, objective journalism." +
           "\r\nBorn October 4, 1993" +
           "\r\nEducation: " +
           "\r\nKornberg School of Journalism - De Groot University, Class of 2016 (Bachelor of Arts)" +
           "\r\nWork History:" +
           "\r\nBuffington Post 2016-2017" +
           "\r\nSpaceship News Network 2018-Present\n",

           "<color=\"blue\">SPACESHIP NEWS NETWORK</color>\r\n" +
           "Arm Yourself with Knowledge." +
           "\r\nTOP STORIES:" +
           "\r\nAliens are feared to have infiltrated the ship, impersonating crewmates." +
           "\r\n“Imposters are sabotaging our efforts”, officials report. " +
           "\r\nImposters are unable to complete tasks." +
           "\r\nNews anchor Bobby Doubt sued by local crewmates for defamation." +
           "\r\nAuthorities: “Report any suspicious activity (or dead bodies) immediately”.\r\n",

           "<color=\"blue\">BLUFFINGTON POST</color>\r\n" +
           "Completely Objective, Absolutely Accurate. 100%." +
           "\r\nTOP STORIES:" +
           "\r\nARE IMPOSTERS REALLY THAT BAD? THIS NEW REPORT WILL SHOCK YOU" +
           "\r\nDISGUSTING!!! Blue crewmate caught SHOWERING in TOILET?" +
           "\r\nAnonymous Source: “We are completely safe”. Is everyone overreacting?" +
           "\r\nGreen crewmate has done ZERO tasks, keeps slipping on banana peel. " +
           "\n“It was their own banana”, says Red.\r\n",

           "<color=\"blue\">IMPOSTER KYLE'S PERSONAL BLOG</color>\r\n" +
           "@imposterkylespersonalbl0g" +
           "\r\nAbout me:" +
           "\r\nWhatup Im Imposter Kyle, i’ve been working on the crew for yearsss. I like doing my tasks, participating in meetings " +
           "\nand definitely not sneaking up on my crewmates when they’re alone. I also like cereal and doing sick flips on my skateboard " +
           "\nin the parking lot XD Hit subscribe for more of my thoughts and dreams, my plans and my schemes!!!!!1!1" +
           "\r\nPOSTS" +
           "\r\n10/25/2013: Im SOOOO tired of Purple…" +
           "\r\nJust got out of a meeting and purple was completely on my case. Accusing me, saying i vented JUST BECAUSE i was in th vents. " +
           "\nI JUST LIKE SQUIRMING AROUND THE VENTS LIKE A BUG!!! Ooh if I see Purple walking around….\r\n"
       };

    }
}
