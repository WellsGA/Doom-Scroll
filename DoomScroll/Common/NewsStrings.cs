
namespace Doom_Scroll.Common
{
    internal static class NewsStrings
    {

        // 1) PLAYER GENERATED
        // TRUSTWORTHY - protect
        public static string[] trustProtect = new string[]
        {
            "I've seen {X} do several tasks. Crewest mate I've ever seen.",
            "My cat Sir Wigglesworth loves {X}, and Big Wigs hates everyone.",
            "I've done {#} tasks! I'M INNOCENT.",
            "I'm crew I'm clean I'm vibin.",
            "I'm signed in to {#} tasks obviously I'm crew."
        };

        // TRUSTWORTHY - frame
        public static string[] trustFrame = new string[]
        {
            "{X} is looking pretty sus",
            "I might be imposter tbh"
        };
        // UNTRUSTWORTHY - protect
        public static string[] unTrustProtect = new string[]
        {
            "My cat trusts {X}, I trust {X}. Ms. Meatball can sense good auras",
            "{X} just saved my puppy from a burning building!",
            "{X} just saved the ship from sabotage #hero",
            "I can't believe {X} and {Y} are DATING #scandalous"
        };
        // UNTRUSTWORTHY - frame
        public static string[] unTrustFrame = new string[]
        {
            "{X} seen stealing food from puppies!",
            "Is {X} really who they say they are?",
            "{X} Isn't doing their fair share!",
            "{X} just stole my lunch from the fridge :(",
            "{X} KEPT ME UP ALL NIGHT! STOP PARTYING PLEASE",
            "[S] says {X} hasn't done ANY tasks???? INSANE",
            "I just caught {X} loitering near vents???"
        };

        // 2) AUTO GENERATED
        // TRUSTWORTHY - protect
        public static string[] autoTrustProtect = new string[]  // NEEDS VALIDATION
        {
            "{X} has signed in to {#} tasks.",
            "{X} Saves Ship from Sabotage.",
            "{X} Has Completed Several Tasks.",
            "Security Records Reveal {X} Has Prevented # Sabotages",
            "{X} has completed {#} tasks"
        };
        // TRUSTWORTHY - frame
        public static string[] autoTrustFrame = new string[]
       {
            "Task Records Show {X} Hasn't Completed Their Tasks",
            "{X} is Missing from Sign-In Sheet",
            "{X} Seen Loitering Near Vents.",
            "Report: {X} Committed Sabotage.",
            "'{X} CANNOT Be Trusted', Local Ghost Warns."
       };
        // UNTRUSTWORTHY - protect
        public static string[] autoUnTrustProtect = new string[]
       {
           "HERO: {X} Saves Ship from Sabotage.",
           "{X} saves Puppy from Burning Building!"
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
        public static string[] trustSource = new string[]
       {
           "{C} Crewmate",
           "@{N}"
       };

        public static string[] unTrustSource = new string[]
        {
            "{C} Crewm8_",
            "@NotMe29",
            "@FreddyGazebo1922830485",
            "@xXCrewhateXx",
            "{C} Grewmate",
            "@{N}_1515386",
            "@REAL{N}",
            "@{N}official"
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
