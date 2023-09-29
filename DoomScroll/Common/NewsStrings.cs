
namespace Doom_Scroll.Common
{
    internal static class NewsStrings
    {
        // 1) PLAYER GENERATED
        // UNTRUSTWORTHY - protect
        public static string[] unTrustProtect = new string[]
        {
            "{S} is complete mainstream propaganda fake news",
            "I can't believe anyone trusts {S}",
            "Anyone who believes {S} is a sheep a baby baby lamb",
            "My cat trusts {X}, I trust {X}. Ms. Meatball can sense good auras",
            "{X} just saved my puppy from a burning building!",
            "{X} just saved the ship from sabotage #hero",
            "{X} is a superhero sent to protect us we luv {X} <3  <3 <3",
            "I can't believe {X} and {Y} are DATING #scandalous"
        };

        // UNTRUSTWORTHY - frame
        public static string[] unTrustFrame = new string[]
        {
            "{S} is complete mainstream propaganda fake news",
            "I can't believe anyone trusts {S}",
            "Anyone who believes {S} is a sheep a baby baby lamb","{X} seen stealing food from puppies!",
            "Is {X} really who they say they are?",
            "{X} Isn't doing their fair share!",
            "{X} just stole my lunch from the fridge :(",
            "{X} KEPT ME UP ALL NIGHT! STOP PARTYING PLEASE",
            "{X} says {Y} hasn't done ANY tasks???? INSANE",
            "I just caught {X} loitering near vents???"
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
                 "\"{X} is no longer a suspect\", local investigators state." // role
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
           "Streets safe again: Our very own innocent {X} didn't kill anyone last round!!!"
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
            "TENSION ON THE SHIP! {X} seen arguing with Y."
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
            "@{NR}"
        };


        public static string[] autoTrustSource = new string[]
       {
           "Spaceship News Network",
           "Amongus Weekly",
           "Buffington Post",
           "Crewmate Community Board",
           "ZotZotZot Chronicles",
           "Julia the Journalist",
           "Crews News",
           "The Gopher Report"

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
           "@GLSNews",
           "The Selfless Report"
       };

  }
}
