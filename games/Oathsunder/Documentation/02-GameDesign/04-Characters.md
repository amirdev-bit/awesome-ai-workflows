# OATHSUNDER — Characters

> **Phase 2 · Game Design Document · Cast**
> Source of truth: [`../00-Canon.md`](../00-Canon.md). The world and choices are in
> [`02-World-and-Narrative.md`](02-World-and-Narrative.md), where each character appears is in
> [`03-Missions.md`](03-Missions.md), and combat and rewards for bosses are in [`05-Bosses.md`](05-Bosses.md).

---

## 0. Conventions

| Rule | Detail |
|---|---|
| IDs | The ID column holds each character's primary ID. Canon characters keep their canon IDs: `char.*` for the protagonist and companions, `boss.*` for the 24 bosses. New characters get new `char.*` IDs. |
| VO speaker IDs | A boss speaks in dialogue data as `char.<short>`, where `<short>` is the boss ID after `boss.` (e.g. `boss.kessh` speaks as `char.kessh`). This matches canon's split between `char.rhen` and `fighter.rhen`. `boss.petals` speaks as `char.ysolde` and `char.yrrah`. |
| Faction | Canon factions only: The Solemn Throne, Oathwardens, The Unsworn, The Ashen Choir, Lantern Guild, The Rootbound, Free Blades. People outside all of them are **Unaffiliated**. A household or sub-group is given in parentheses. |
| Region | The canon region ID where the character is based or first met. *Undermourn* marks characters met in the drowned realm. |
| Voice direction | Apparent age, timbre, delivery. Casting notes are for the VO director, and every actor must be able to deliver the performance in all shipped languages' reference reads. |
| Count | **93 characters**: 8 protagonist and companions, 24 canon bosses, 25 lieutenants and villains, and 36 allies, merchants, quest givers, rivals and townsfolk with story weight. |

---

## 1. Roster

### 1.1 Protagonist and companions (canon)

| ID | Name | Faction | Region | Role | One-line personality | Voice direction |
|---|---|---|---|---|---|---|
| `char.rhen` | **Rhen** | Oathwardens (former), now Oathsundered | `region.emberfall` | Protagonist. Former Oathwarden, executed for refusing to tithe a living child's shadow, returned from the Undermourn bound by no oath. Wields Ember and Umbra together. Default weapon: Katana. | Quiet, stubborn, dry. He keeps promises to people, not institutions. | Early 30s. Low baritone, worn and warm. Few words, understated, and never shouts except in Ember Rage. |
| `char.sable` | **Sable** | The Unsworn (Rhen's severed shadow) | `region.emberfall` | Companion. The sardonic voice in Rhen's head and his physical partner in Umbral Shadow. His boss form is `boss.sableascendant`. | Mocking and hungry for sensation, and secretly terrified of being cut away again. | **The same actor as Rhen**, recorded close-mic'd and dry, delivered faster, with a grin you can hear. Whispers with teeth. |
| `char.liss` | **Liss** | Unaffiliated | `region.weepingreeds` | Companion. The child whose shadow came loose early. Key to the true ending. | Solemn, curious, and blunt the way only an eight-year-old can be. Hums when she is frightened. | Age 8 (cast age 9–11). Clear, small, unhurried. Speaks in whole sentences, like someone who has been alone a lot. |
| `char.mireth` | **Mireth Vale** | Oathwardens (disgraced archivist) | `region.silkwind` | Companion. Runes, enchantments and lore. | Precise and prickly, and funny when she forgets herself. Cannot leave a wrong footnote alone. | Mid-40s. Crisp alto, quick, with clipped consonants. Lectures when she's nervous. |
| `char.tessen` | **Tessen** | Unaffiliated (formerly Ironroot guild) | `region.ironroot` | Companion. Blacksmith, crafting and weapon mastery. | Gruff. Patient with metal and impatient with people, and hammers his guilt into every blade. | Late 60s. Gravel bass, slow, breathy from forge smoke. Long pauses. |
| `char.adekan` | **Brother Adekan** | The Ashen Choir (defector) | `region.emberfall` | Companion. Trainer, and host of Training mode and the Dojo. | Gentle and disciplined, carrying a rage he calls penance. | Late 30s. A resonant tenor trained for chant. Measured and calm, and it cracks when he's angry. |
| `char.oskar` | **Oskar Dray** | Free Blades (captain of the Salt Crows) | `region.ironroot` | Companion. Arena, Survival and Clan contracts. | Loud and generous. Mercenary on paper and sentimental in practice. | Mid-40s. Big warm baritone with a gravelly laugh and fast banter, and a Rimewood burr underneath. |
| `char.neve` | **Neve** | Lantern Guild | `region.lanternhold` | Companion. Trader: cosmetics, collectibles and weekly events. First met in the Weeping Reeds. | Charming and calculating. Loyal to exactly three people and pretends it's two. | Late 20s. Bright mezzo, quick, smiling through her teeth. Drops into flat seriousness when it matters. |

### 1.2 The Oathlords (canon bosses 1–12)

| ID | Name | Faction | Region | Role | One-line personality | Voice direction |
|---|---|---|---|---|---|---|
| `boss.kessh` | **Abbot Kessh** | The Ashen Choir | `region.emberfall` | Oathlord of Devotion, the Ash Censer. Head of the Ashen Choir, and the man who presided over Rhen's execution. | Serene and devout, and he truly loves the souls he burns. | Late 60s. Deep, soft cantor's voice that is never raised. Every sentence sounds like a benediction. |
| `boss.ilvane` | **Mother Ilvane** | The Solemn Throne (Weeping Reeds household) | `region.weepingreeds` | Oathlord of Mercy, the Drowned Bell. Matriarch of the Houses of Quiet Water. | Tender, exhausted, and certain that drowning is a kindness. | Late 60s. Low contralto in a lullaby cadence, wet and breathy. Hums between lines. |
| `boss.gorran` | **Gorran Vox** | The Solemn Throne (Ironroot) | `region.ironroot` | Oathlord of Labor, the Anvil King. Led the Cinder Rising and then doubled the Shadow Shifts. | Proud and pragmatic. A revolutionary who became the ledger he once burned. | 50s. Booming bass with working-class vowels. His delivery falls into the hammer rhythm (long, short, short). |
| `boss.petals` | **Ysolde & Yrrah** | The Solemn Throne (Silkwind loom-houses) | `region.silkwind` | Oathlord pair of Kinship, the Paired Petals. Ysolde is living. Yrrah is her dead twin, whose shadow Ysolde has bound to her own. | Ysolde: fierce, tender, and unable to let go. Yrrah: faded, gentle, and tired of being held. | Both early 30s. One actor double-tracked, or twins. Ysolde is bright and sharp. Yrrah is the same voice a half-step lower, a beat late, with reverb. |
| `boss.maal` | **Seraph Maal** | The Solemn Throne (Faith) | `region.glassossuary` | Oathlord of Faith, the Sunscorched. Blinded himself looking for Aurem in the sun. | Ecstatic, blind, and generous. A zealot who truly believes he is saving you. | 40s. A soaring preacher's tenor in call-and-response rhythm, hoarse from sermons. |
| `boss.tharuk` | **Tharuk Greymane** | The Solemn Throne (Greymane clans) | `region.rimewood` | Oathlord of Strength, the Winter Wolf. Founder of the Culling Moots. | Hard and fair by his own measure. Grieves for every person he has culled. | 60s. Huge rough bass with growled consonants and long silences. He speaks like he is counting. |
| `boss.veloran` | **Duke Veloran Sae** | The Solemn Throne (House Sae) | `region.lanternhold` | Oathlord of Loyalty, the Masquerade. Enforcer of the Masque Oath, and the man who hired Tamsin. | Witty, theatrical and hollow. He has worn masks so long there is nothing left under them. | 40s. Silky baritone, playful and sing-song. Every line sounds like a toast. |
| `boss.myrrhen` | **Queen Myrrhen** | The Rootbound | `region.verdantrot` | Oathlord of Life, the Mycelial Bride. Married to the Rot at nineteen and kept alive by it. | Dreamy and kind, and horrifyingly patient. Loves everything too much to let it end. | Sounds 19. Soft soprano with a faint choir of whispers layered under it. Slow. |
| `boss.oru` | **Master Oru** | The Solemn Throne (Still Wind school) | `region.cloudspire` | Oathlord of Discipline, the Still Wind. Has not moved his feet in combat for forty years. | Calm and exact. A dam holding back a flood. | 70s. Quiet, dry tenor. Few words and long pauses, and he never uses contractions. |
| `boss.quill` | **Archivist Quill** | The Solemn Throne (Archive) | `region.sunkenarchive` | Oathlord of Memory, the Thousand Pages. Keeper of the Ledger of the Tithed, and Mireth's old master. | Fussy, brilliant and frightened. Hoards memory because he has lost his own. | 80s. Papery, thin baritone, rattling off citations in a whisper. Sometimes loses his place mid-sentence. |
| `boss.hask` | **General Hask Varrow** | Oathwardens / The Solemn Throne | `region.bloodmoon` | Oathlord of Duty, the Crimson Vow. Lord-Commander of the Oathwardens, Rhen's mentor, and the signer of his death writ. | Iron discipline and a deep love for Rhen, with a deeper loyalty to the wall. | 60s. A commanding gravel baritone that barks clipped orders. It softens only when he says "boy". |
| `boss.aurem` | **Emperor Aurem** | The Solemn Throne | `region.solemnthrone` | Oathlord of Sovereignty, the Solemn Sun. First emperor, deathless for five hundred years. | Magnificent, lonely and utterly certain. The saddest man in Varanth, and he knows it. | Timeless, sounds about 50. A resonant, perfectly articulated baritone, never hurried. A faint doubled echo with no source, because he has no shadow to return it. |

### 1.3 Legendary bosses (canon bosses 13–24)

| ID | Name | Faction | Region | Role | One-line personality | Voice direction |
|---|---|---|---|---|---|---|
| `boss.tamsin` | **Tamsin** | Lantern Guild (contract assassin, of the Nine Knives) | `region.lanternhold` | Recurring rival in regions 2–7 and boss of the final rooftop duel. Last of Lady Corvaine's nine apprentices. | Wry, professional and lonely. Counts every debt, her own included. | Late 20s. Low, husky alto, deadpan. Fast when she lies and slow when she's honest. |
| `boss.sableascendant` | **Sable Ascendant** | The Unsworn | Undermourn | Sable in the mirror duel, "the Other You". | Everything Sable kept hidden: jealousy, tenderness, and wanting to be the one who lives. | Rhen's actor **unprocessed**, so Sable speaks in Rhen's clear voice for the first time. Gentle, which is what makes it frightening. |
| `boss.ferrousmaw` | **The Ferrous Maw** | The Solemn Throne (Ironroot engine) | `region.ironroot` | Raid boss, the Engine of Ironroot. Driven by a thousand clamped shadows. | No mind of its own, only a thousand exhausted minds that want to stop. | No lines. A chorus of 40 workers, men and women, chanting the shift song, processed through pipes and bellows. |
| `boss.corvaine` | **Lady Corvaine** | The Unsworn (Remnant) | `region.lanternhold` | Hidden duel, the Ninth Widow. Tamsin's teacher, executed for the one husband she didn't kill. | Elegant, venomous and grieving. | 50s, her age at death. Rich, smoky contralto, aristocratic and amused. Weeps without losing her cadence. |
| `boss.ossric` | **Ossric** | Unaffiliated (pre-Oath Grave-Warden) | `region.glassossuary` | Hidden duel, the Grave-Warden. The last guide of the Old Road, from before the Oath. | Ancient, courteous and weary. Has waited 500 years for someone to hum the right song. | Very old. A deep, cracked bass like shifting sand. Archaic phrasing, slow. |
| `boss.drownedchoir` | **The Drowned Choir** (Hesper, Lark and Orla) | The Unsworn | `region.weepingreeds` | Raid boss, "Three Voices, One Throat". Mother Ilvane's three daughters, drowned mid-hymn in 471 AS. | Three girls caught in the last hymn they ever sang. They want their mother, and they want to stop singing. | Three girls (15, 13 and 11) in close harmony, with lines split word by word between them. Always sung, never spoken. |
| `boss.velka` | **Iron Abbess Velka** | The Ashen Choir (founding era) | `region.ironroot` | Hidden duel, the Unquenched. Forged the first tithe-censers two centuries ago and never cooled. | Fanatical, proud, and worn out by two hundred years of burning. | Ageless. A powerful alto with a metallic ring. Speaks in liturgy. |
| `boss.eirmund` | **Eirmund** | The Unsworn (Remnant) | `region.rimewood` | Hidden duel, the Pale Stag. A hunter-prince from before the Greymanes, guarding the Rimewood dead. | Noble, courteous, a tireless hunter. | Early 20s, his age at death. Clear, light tenor with frost in the breath. Formal speech. |
| `boss.senajari` | **Sen Ajari** | Unaffiliated (oathless hermit) | `region.cloudspire` | Hidden duel, the Lotus Hermit. Oru's master, sixty years at the summit. | Playful, serene and disarming. Laughs at his own lessons. | 90s. Light, merry tenor, very soft. Laughs in the middle of sentences. |
| `boss.isketh` | **Isketh** | The Rootbound (the first queen, cast out) | `region.verdantrot` | Hidden duel, the Marrow Queen. Walled up in the Marrow Hollows by her own cult. | Ravenous, patient, and contemptuous of Myrrhen's "gentleness". | Ageless. A clicking, layered whisper over a low female voice, with hissed sibilants. Slow and hungry. |
| `boss.hundredhanded` | **The Hundred-Handed Warden** | Oathwardens (the first hundred) | Undermourn | Raid boss, Keeper of the Gate. A hundred Oathwardens who swore to guard the Gate from inside, fused into one body. | Loyal past all sense. Remembers every one of its hundred names. | A chorus of a hundred men and women reciting names. Spoken lines come from one lead voice with 99 whispering beneath it. |
| `boss.firstshadow` | **The First Shadow** | The Unsworn (the keystone) | Undermourn | True final boss, Umbra Prime. Aurem's severed shadow, swollen with five hundred years of tithe. | Vast hunger and vaster grief. All it wants is to be taken back. | Aurem's actor, slowed and pitched down, layered with thousands of tithed voices. It rarely speaks, and every line lands like a bell. |

### 1.4 Lieutenants and villains

| ID | Name | Faction | Region | Role | One-line personality | Voice direction |
|---|---|---|---|---|---|---|
| `char.vashti` | **Precentor Vashti Coil** | The Ashen Choir | `region.emberfall` | Kessh's lieutenant, hymn-mistress and censer captain (`mission.emberfall.06`). Returns in `quest.adekan.04`. | Zealous and exacting. Fears silence more than death. | 40s. Sharp soprano. Sings her commands with cold precision. |
| `char.horvath` | **Deacon Horvath Pyle** | The Ashen Choir | `region.emberfall` | Kessh's lieutenant. The headsman who executed Rhen (`mission.emberfall.08`). | Methodical and pious. Quietly proud of a clean cut. | 50s. Flat, heavy baritone in a deadpan prayer-murmur. |
| `char.collum` | **Ferryman Collum Brack** | The Solemn Throne (Ilvane's household) | `region.weepingreeds` | Ilvane's lieutenant. Poles the funeral barges (`mission.weepingreeds.07`). | Superstitious, greedy, and afraid of the water he works on. | 50s. Wet rasp with a marsh accent. Mutters counting rhymes. |
| `char.oona` | **Sister Oona Weir** | The Solemn Throne (Houses of Quiet Water) | `region.weepingreeds` | Ilvane's lieutenant. Head of the House of Quiet Water (`mission.weepingreeds.04`). | Motherly, sincere and monstrous. Believes every drowning is a gift. | 40s. Soft, nurturing mezzo in a sing-song of reassurance. |
| `char.brisa` | **Forewoman Brisa Kell** | The Solemn Throne (Ironroot) | `region.ironroot` | Gorran's lieutenant, "the Tallyhand", who counts every shadow in the city (`mission.ironroot.08`). | Brisk and numerate. Thinks numbers are fair because they don't care. | 40s. Crisp, fast, flat mid-alto. Speaks in figures. |
| `char.dunmarrow` | **Overseer Dunmarrow** | The Solemn Throne (Ironroot) | `region.ironroot` | Gorran's lieutenant. A slag-armored enforcer (`mission.ironroot.09`). | Brutal, simple, and loyal to Gorran the way a hammer is to a hand. | 40s. Subterranean bass under a riveted helm, with metallic resonance. Few words. |
| `char.saffi` | **Loom-Mother Saffi Orle** | The Solemn Throne (Silkwind loom-houses) | `region.silkwind` | The Petals' lieutenant. Mistress of the Great Loom and the Binding Rite (`mission.silkwind.08`). | Elegant and severe. Believes grief should be woven, never cut. | 60s. Dry, precise alto. Speaks like she's counting stitches. |
| `char.lorcan` | **Captain Lorcan Vey** | The Solemn Throne (Thornsworn) | `region.silkwind` | The Petals' lieutenant. Captain of the Thornsworn bodyguard (`mission.silkwind.07`). | Devoted to Ysolde, and secretly in love with Yrrah's memory. | 30s. Warm, controlled, courtly tenor. |
| `char.ammun` | **Lector Ammun Sayre** | The Solemn Throne (Faith) | `region.glassossuary` | Maal's lieutenant, "the Glass Tongue" (`mission.glassossuary.04`). | Silver-tongued, with cynicism under the zeal. Sells salvation by the ounce. | 50s. Honeyed baritone in a preacher's cadence, with a wink you can hear. |
| `char.sethe` | **Ghafir Sethe** | The Solemn Throne (the Glassblind) | `region.glassossuary` | Maal's lieutenant. Captain of the Glassblind (`mission.glassossuary.09`). | Stoic and disciplined. Has found peace in the dark. | 30s. Low and even, with long listening pauses and whispers. |
| `char.kirra` | **Kirra Ashfang** | The Solemn Throne (Greymane clans) | `region.rimewood` | Tharuk's lieutenant and adopted daughter. Possible Keeper of Strength (`mission.rimewood.08`). | Proud and fierce. Loves her father and hates the Moots. | Mid-20s. Strong, husky mezzo with a steppe accent. Direct and fast. |
| `char.hrolm` | **Hrolm the Skyreader** | The Solemn Throne (Greymane clans) | `region.rimewood` | Tharuk's lieutenant. The shaman who reads the aurora and chooses the culled (`mission.rimewood.07`). | Mystical, cruel out of habit, and afraid of the black aurora. | 70s. Reedy, wavering tenor that chants, then turns suddenly sharp. |
| `char.ormond` | **Masque-Captain Ormond Lisle** | The Solemn Throne (the Duke's Silks) | `region.lanternhold` | Veloran's lieutenant, "the Smiling Mask" (`mission.lanternhold.06`). | Vain and charming. Cruel for applause. | 30s. Bright theatrical tenor with stage projection. Laughs on every third line. |
| `char.vivienne` | **Vivienne Sae** ("the Pale Mask") | The Solemn Throne (House Sae) | `region.lanternhold` | Veloran's lieutenant, his sister and spymaster (`mission.lanternhold.07`). | Cool and perceptive. The only Sae who knows what is under her brother's mask. | 40s. Low, quiet, intimate contralto. Never wastes a word. |
| `char.anselk` | **Anselk the Groom** | The Rootbound | `region.verdantrot` | Myrrhen's lieutenant, her husband for eighty years, held upright by the mycelium (`mission.verdantrot.08`). | Devoted, slow, long past himself. Keeps repeating his wedding vows. | Sounds 30 and 110 at once. Hollow baritone with creaking wood in it. Slow repetitions. |
| `char.vell` | **Sporemother Vell** | The Rootbound | `region.verdantrot` | Myrrhen's lieutenant and high priestess (`mission.verdantrot.05`). | Rapturous, maternal, and fanatical about "no endings". | 50s. Breathy, swooning mezzo, with whispering spores in the mix. |
| `char.imre` | **Imre Tal** | The Solemn Throne (Still Wind school) | `region.cloudspire` | Oru's lieutenant and First Student. Possible Keeper of Discipline (`mission.cloudspire.07`). | Flawless, hollow, and desperate to feel anything. | Early 20s. Flat, perfect, calm tenor that cracks as his feelings come back. |
| `char.wren` | **Sister Wren** | The Solemn Throne (Still Wind school) | `region.cloudspire` | Oru's lieutenant, Bridge-Keeper of the Ninth Bridge. Fallback Keeper of Discipline (`mission.cloudspire.04`). | Kind, dutiful, and wise enough to doubt. | 50s. Warm, low, unhurried mezzo. |
| `char.salk` | **Curator Salk** | The Solemn Throne (Archive) | `region.sunkenarchive` | Quill's lieutenant, keeper of the Drowned Stacks (`mission.sunkenarchive.05`). | Pedantic, territorial and lonely. | 60s. Nasal, fussy tenor. Shushes people. |
| `char.index` | **The Index** | The Solemn Throne (Archive) | `region.sunkenarchive` | Quill's lieutenant. A scholar with a thousand catalog tags sewn into his skin (`mission.sunkenarchive.08`). | Encyclopedic, detached and unsettlingly polite. | Ageless. Toneless, precise baritone, read like a card catalog, with whispered citations overlapping underneath. |
| `char.idris` | **Warden-Captain Idris Hale** | Oathwardens | `region.bloodmoon` | Hask's lieutenant. Rhen's sworn brother and rival. Possible Keeper of Duty (`mission.emberfall.01`, `.09`; `mission.bloodmoon.02`, `.08`). | Loyal and bitter. Loves Rhen and cannot forgive him. | Early 30s. Clean, strong baritone. Formal, until he slips back into old familiarity. |
| `char.ulla` | **Siege-Marshal Ulla Brandt** | The Solemn Throne (Crimson Vowguard) | `region.bloodmoon` | Hask's lieutenant, commander of the walls. Fallback Keeper of Duty (`mission.bloodmoon.05`). | Pragmatic and blunt, with gallows humor. | 50s. Loud alto, hoarse from shouting over siege fire. Clipped. |
| `char.orsolya` | **Chancellor Orsolya Venn** | The Solemn Throne | `region.solemnthrone` | Aurem's lieutenant. Author of the Edict of the Living Tithe, and the one who put out Tamsin's contract (`mission.solemnthrone.02`, `.07`). | Brilliant and ruthless. Sincerely believes she is saving the empire. | 50s. Cultured, precise alto, reasonable to the point of horror. |
| `char.casimir` | **High Tithe-Reader Casimir Dole** | The Solemn Throne | `region.solemnthrone` | Aurem's lieutenant, keeper of the Tithe Chancery (`mission.solemnthrone.04`). | Bureaucratic and meticulous. Has never looked up from the ledger. | 60s. Dry, monotone tenor. Is always reading something aloud. |
| `char.blaise` | **Ser Blaise Ondrey** | The Solemn Throne (Sunward Guard) | `region.solemnthrone` | Aurem's lieutenant, captain of the Sunward Guard (`mission.solemnthrone.05`). | Honorable and ceremonial. Afraid he has never really been tested. | 40s. Noble, ringing, formal baritone. |

### 1.5 Allies, merchants, quest givers, rivals and townsfolk

| ID | Name | Faction | Region | Role | One-line personality | Voice direction |
|---|---|---|---|---|---|---|
| `char.silt` | **Old Mother Silt** | The Unsworn (Remnant) | Undermourn | Prologue guide. A midwife from before the Oath (`mission.emberfall.03`). | Warm, salty and ancient. Calls everyone "love". | Very old. Low, cracked, warm alto. Her laugh sounds like water going down a drain. |
| `char.idra` | **Sister Idra** | The Ashen Choir | `region.emberfall` | Quest giver who tends the Ash Gardens. Keeper of Devotion on Restore. | Kind and stubborn, and braver than she knows. | 30s. Soft, clear mezzo. Hesitant, then firm. |
| `char.pell` | **Old Pell** | Unaffiliated | `region.emberfall` | Merchant who sells candles at the monastery gate (`side.emberfall.01`). | Chatty and cheerful, and horrified to learn what his wax is made of. | 70s. Creaky, merry tenor that rambles. |
| `char.soren` | **Brother Soren** | The Ashen Choir | `region.emberfall` | Adekan's friend inside the Choir. Runs the Dojo if Adekan leaves. | Timid and loyal. Sings when he's scared. | 30s. Light tenor with a stammer. |
| `char.gudrun` | **Gudrun** | Unaffiliated | `region.weepingreeds` | Quest giver, an eel-wife who hides loose-shadowed children. Keeper of Mercy on Restore. | Gruff, sharp-tongued, and fiercely protective. | 60s. Rough, salt-cured alto in marsh dialect. |
| `char.pim` | **Pim** | Unaffiliated | `region.weepingreeds` | A boy in the House of Quiet Water and Liss's first friend. | Brave and frightened. Jokes so he won't cry. | Age 10. Bright, nervous and chatty. |
| `char.hanne` | **Hanne** | Unaffiliated (tithed, unwritten) | `region.weepingreeds` | Liss's mother, heard only through Shadow Echoes. The first shadow returned in the Rewoven Oath. | Tender, tired, and fearless for her daughter. | 30s. Soft, husky alto. The Rebirth Song is sung in her voice. |
| `char.moth` | **Moth** | The Unsworn (Liss's loose shadow) | `region.weepingreeds` | Liss's loose shadow, a child-sized silhouette that wanders. | Curious, skittish and affectionate. | No words. Foley of breaths and giggles, and a hummed counter-melody to Liss. |
| `char.juno` | **Juno Tallow** | Unaffiliated (the Cinder Strike) | `region.ironroot` | Quest giver and strike leader. Keeper of Labor on Restore. | Fiery and practical. Allergic to crowns. | 30s. Strong, rough alto in a speech-maker's cadence. |
| `char.abasi` | **Abasi Rook** | Lantern Guild | `region.ironroot` | Merchant trading ore and grit. | Shrewd, and honest about his dishonesty. | 50s. Rich, rolling baritone with bargaining patter. |
| `char.dari` | **Dari** | Unaffiliated | `region.ironroot` | Tessen's apprentice. Runs the forge if Tessen leaves. | Eager, clumsy, and in awe of Rhen. | Age 16. Quick voice that cracks. Says "sir" too often. |
| `char.mags` | **Mags Harrow** | Free Blades (the Salt Crows) | `region.ironroot` | Oskar's second. Handles contracts if Oskar leaves. | Dry and loyal. Counts everything twice. | 40s. Deadpan, gravelly alto. One-liners. |
| `char.ludo` | **Captain Ludo Skarre** | Free Blades (the Red Tallies) | `region.ironroot` | Rival captain who sells contracts to the highest bidder (`mission.ironroot.06`, `quest.oskar.02`). | Smug and capable, and a coward once the coin runs out. | 40s. Nasal, oily baritone with a sneering laugh. |
| `char.grest` | **Grest** | Free Blades | `region.ironroot` | Pit-master and announcer of the Slag Pits (`side.ironroot.03`). | A showman, greedy, who loves a good fight. | 50s. An enormous announcer's bellow. Rhymes his introductions. |
| `char.amaru` | **Amaru** | Unaffiliated | `region.silkwind` | Quest giver, the Wind-Listener who tunes the chimes (`mission.silkwind.02`). | Serene and teasing. Deaf in one ear. | 70s. Soft, musical tenor. Hums pitches. |
| `char.cassia` | **Cassia** | Unaffiliated (loom-house) | `region.silkwind` | Quest giver, a silk-dyer. Keeper of Kinship on Restore. | Determined and warm, dye-stained to the elbows. | 20s. Bright soprano that speeds up when she's determined. |
| `char.cato` | **Cato** | Unaffiliated (loom-house) | `region.silkwind` | Cassia's twin, freed from the Binding Rite (`mission.silkwind.09`). | Gentle, withdrawn and grateful. | 20s. Soft, slow tenor. Often finishes Cassia's sentences. |
| `char.yusra` | **Yusra Dahl** | Unaffiliated | `region.glassossuary` | Quest giver, a bone-cartographer. Keeper of Faith on Restore. | Rigorous and wry. Trusts maps more than gods. | 40s. Dry, precise, measured mezzo. |
| `char.obed` | **Obed** | Lantern Guild | `region.glassossuary` | Merchant who sells water (`mission.glassossuary.02`). | Greedy and cowardly, and secretly generous to children. | 50s. Wheedling high baritone. Complains constantly. |
| `char.mathilde` | **Ser Mathilde Crowe** | Oathwardens (retired) | `region.rimewood` | Rhen's first teacher. Gives him trials (`mission.rimewood.02`). | Stern and funny, and proud of the man who broke her order's rules. | 70s. Commanding, weathered alto in a teacher's rhythm. |
| `char.yeva` | **Yeva Sarn** | Unaffiliated (clanless) | `region.rimewood` | Quest giver, a widow who shelters the culled. Fallback Keeper of Strength. | Hard, hospitable and unbending. | 50s. Low, rough mezzo with a steppe accent. |
| `char.bodil` | **Bodil** | Unaffiliated (Greymane clan) | `region.rimewood` | A one-legged tanner who remembers when Tharuk was just. | Bitter, loyal and honest. | 60s. Gruff tenor with a storyteller's slowness. |
| `char.tobin` | **Tobin Dray** | The Unsworn (culled) | `region.rimewood` | Oskar's little brother, culled at a Moot. Heard through a Shadow Echo (`quest.oskar.03`). | Sweet and hopeful. Worshipped his brother. | Age 14, his age at death. Light tenor that cracks. |
| `char.fenwick` | **Guildmaster Fenwick Oare** | Lantern Guild | `region.lanternhold` | Head of the Lantern Guild (`quest.neve.05`). | Genial and ruthless. Never raises his voice, or his prices, without a reason. | 60s. Plummy, smooth, avuncular baritone. |
| `char.hollis` | **Hollis Crane** | Lantern Guild | `region.lanternhold` | Villain who runs the shadow market (`mission.lanternhold.05`, `quest.neve.03`). | Soft-spoken and predatory. Thinks of himself as a banker. | 40s. Gentle, whispery, unnervingly polite tenor. |
| `char.quince` | **Madame Quince** | Lantern Guild | `region.lanternhold` | Mask-maker and merchant. Runs Neve's stall if Neve leaves. | Theatrical and perceptive. Reads faces for a living. | 60s. Husky, dramatic alto that rolls its r's. |
| `char.dov` | **Dov** | Unaffiliated (lamplighters) | `region.lanternhold` | Quest giver, an old lamplighter. Keeper of Loyalty on Restore. | Quiet and steady. Knows every roof in the city. | 70s. Soft, slow baritone. Says little. |
| `char.evander` | **Evander Crale** | The Solemn Throne (lapsed naturalist) | `region.verdantrot` | Quest giver, a half-rooted botanist. He promised his daughter Myrrhen to the Rot in 402 AS. Keeper of Life on Restore. | Guilty and curious. Clings to science as penance. | Sounds 70, is 128, preserved by the Rot. Reedy, wheezing tenor that lectures. |
| `char.tuck` | **Tuck** | The Rootbound | `region.verdantrot` | A moss-child and Liss's friend (`mission.verdantrot.07`, `quest.liss.04`). | Solemn and sweet. Thinks everything should grow. | Age 9. Soft, slightly muffled and slow. |
| `char.halloran` | **Halloran Petch** | Unaffiliated (aqueduct engineers) | `region.cloudspire` | Quest giver, the engineer who keeps the bridges afloat (`mission.cloudspire.02`). | Anxious and brilliant. Talks to his machines. | 50s. Fast, nervous tenor with technical muttering. |
| `char.suri` | **Suri** | Unaffiliated (kite-runners) | `region.cloudspire` | A kite-runner courier (`mission.cloudspire.03`). | Reckless and cheerful. The only fast thing in Cloudspire. | Age 13. Quick, breathless soprano. |
| `char.maren` | **Maren Ostrow** | Unaffiliated (divers) | `region.sunkenarchive` | Quest giver, a salvage diver who patches the dome (`mission.sunkenarchive.02`). | Salty and fearless. Counts her breaths out of habit. | 30s. Low, raspy, clipped alto. |
| `char.lotte` | **Lotte Brenn** | The Solemn Throne (Archive, deserter) | `region.sunkenarchive` | Quest giver, a scrivener who flees with the Ledger. Keeper of Memory on Restore. | Timid and principled, and braver every time Rhen sees her. | 20s. Soft, quick mezzo. Apologizes a lot. |
| `char.benedek` | **Benedek Solt** | The Solemn Throne (Citadel quartermaster) | `region.bloodmoon` | Merchant, the Citadel's quartermaster (`mission.bloodmoon.03`). | Weary and decent. Counts rations like prayers. | 50s. Tired, dry baritone. |
| `char.jory` | **Jory Vance** | Oathwardens (recruit) | `region.bloodmoon` | A young Warden who idolized Rhen and wants to desert (`mission.bloodmoon.04`). | Earnest and frightened. Still a little star-struck. | 18. Eager tenor that breaks under stress. |
| `char.tamber` | **Tamber** | Unaffiliated | `region.bloodmoon` | A cook who feeds the Lower Ring (`side.bloodmoon.01`). | Warm, bossy and unbreakable. | 50s. Big, warm, scolding alto. |

---

## 2. Deep profiles: protagonist and companions

### 2.1 Rhen (`char.rhen`)

| | |
|---|---|
| **Background** | Born in 470 AS in the pilgrim town below Emberfall. His mother was tithed when he was nine, and he held the censer-chain while the Choir sang. The Oathwardens took him in, and Hask Varrow made him his favorite. Knighted in 494 AS. |
| **Goal** | First: keep Liss alive and whole. Then: end the tithe without becoming another Aurem. |
| **Fear** | That his refusal was pride and not mercy, and that Liss will pay for it. Underneath that, that he could become a man who owns other people's shadows and calls it protection. |
| **Arc** | From an obedient knight who kept his oaths to institutions, to a man bound by no oath, to someone who has to decide what kind of oath deserves to exist. His relationship with Sable moves from possession to partnership, and the ending decides whether it becomes union (Rewoven), severance (Solemn Dawn), equality in the dark (Unbound Night) or surrender (Sable Ascendant). |
| **Relationship to the world** | The Throne calls him a heretic. The Wardens remember a traitor, and the recruits remember a legend. The Unsworn see the first man ever to climb out whole. |
| **Defining line** | *"I don't break oaths. I just stopped making them for other people."* |

### 2.2 Sable (`char.sable`)

| | |
|---|---|
| **Background** | Rhen's shadow. At the execution the censer smoke reached for him, and he refused, tore himself loose and fell with the body. In the Undermourn he carried Rhen up the Drowned Stair. He remembers things Rhen lost, starting with Rhen's mother's face. |
| **Goal** | To be *someone*: to have weight, sensation and a name, and never to be cut away again. |
| **Fear** | Being severed and dropped into the dark, and becoming what the First Shadow is. |
| **Arc** | Starts as a mocking passenger who treats Rhen's life as a joke. Becomes a partner who feels pain (Tamsin's knife), is tempted by the Remnants ("brother"), learns the truth under the Throne, and asks for Terms. In the mirror duel he offers Rhen his hand. |
| **Relationship to Rhen** | Literally his other half, and the only one who knows everything Rhen has forgotten. Sable's jokes are Rhen's thoughts said out loud with the fear taken out. |
| **Defining line** | *"You got the body. I got the jokes. Seems fair, if you don't think about it."* |

### 2.3 Liss (`char.liss`)

| | |
|---|---|
| **Background** | Born in Hollowmere in the Weeping Reeds in 492 AS. Her mother Hanne died of the Grey Cough in the winter of 499 AS. Liss was holding her hand, and Liss's shadow came loose and caught half of Hanne's before the censer could. She calls her loose shadow **Moth**. |
| **Goal** | To bring her mother home: not back to life, but home to the water. Later, to learn how to make a promise that costs no one. |
| **Fear** | Moth leaving her. Being alone. Being the reason people die, starting with Rhen. |
| **Arc** | From cargo (a child adults carry up mountains and hide in houses) to the person who speaks the Oath again. Through her questline she learns three things: tie a tether you can untie ("lend, not bind"), let things end, and ask for something back. |
| **Relationship to Rhen** | He died for her, and she treats him with total, blunt trust. She is the only one who asks him questions he can't answer ("Does your shadow like you?"). In the true ending she saves him in return. |
| **Defining line** | *"You can hold something without keeping it. Mama did."* |

### 2.4 Mireth Vale (`char.mireth`)

| | |
|---|---|
| **Background** | Oathwarden archivist, trained in the Sunken Archive under Quill. In 491 AS she found the Silence of Births footnote and tried to publish it. Quill disgraced her, the Wardens stripped her, and she has hidden in libraries ever since, most recently the Loom Library. |
| **Goal** | To publish the truth and restore her name. |
| **Fear** | Being right and useless. That the truth won't change anything. |
| **Arc** | From wanting vindication to wanting the truth to serve the living. The key moment is **Publish** (`quest.mireth.05`): she can nail the page to the Vow Hall door and be proven right, or trust a child to read it later. |
| **Relationship to Rhen** | She catalogued his sigil when he was knighted and remembers his handwriting. She treats him as a primary source and slowly starts treating him as a friend. |
| **Defining line** | *"History isn't written by the victors. It's redacted by them."* |

### 2.5 Tessen (`char.tessen`)

| | |
|---|---|
| **Background** | Apprenticed as a boy to Iron Abbess Velka in her hidden foundry, then became Ironroot's master smith. Thirty years ago he forged *Vowcleaver* for Hask Varrow. He fled when Gorran doubled the Shadow Shifts and has worked the Cold Forge alone since. |
| **Goal** | To forge one blade that doesn't serve the tithe. |
| **Fear** | That everything he ever made cut someone who didn't deserve it. |
| **Arc** | From guilt to making something. He faces his teacher (Velka), his best work (Vowcleaver) and his last commission (the *Duskforged Oathblade*, quenched in Undermourn water to hold Ember and Umbra together). |
| **Relationship to Rhen** | A stern grandfather who insists "metal remembers the hand" and makes Rhen hold every blade before he'll sell it. |
| **Defining line** | *"A sword doesn't care what it's for. That's why the smith has to."* |

### 2.6 Brother Adekan (`char.adekan`)

| | |
|---|---|
| **Background** | Orphaned by the tithe, raised by the Ashen Choir, and a Deacon by twenty-five. On the day of Rhen's execution he held the knife meant for Liss's shadow, and put it down. He smuggled her downriver to the House of Quiet Water, believing it a refuge, and has hated himself for that ever since. |
| **Goal** | To atone for every rite he performed, and to write a funeral that takes nothing. |
| **Fear** | His own anger, and the suspicion that he loved the rite's certainty. |
| **Arc** | From penitent to teacher to the composer of **the Returning**. In Cloudspire he learns to win without striking first. At home he sings a new rite in the Belltower where he used to sing the old one. |
| **Relationship to Rhen** | He saw Rhen die. He trains Rhen's body in the Dojo, and Rhen, without meaning to, teaches him to forgive himself. |
| **Defining line** | *"I sang for thirty thousand funerals. I would like to sing for one."* |

### 2.7 Oskar Dray (`char.oskar`)

| | |
|---|---|
| **Background** | Born in Rimewood. His family fled south after his brother Tobin was culled at a Moot. He built the Salt Crows from dock-hands and deserters, and they now owe more than they earn. |
| **Goal** | To keep his crew alive and paid. Later, to find out what happened to Tobin. |
| **Fear** | Caring about a cause, because causes get crews killed. |
| **Arc** | From coin to cause, or to an honest retreat. **Colors** (`quest.oskar.05`) decides whether the Salt Crows ride with Rhen, guard the refugees or go home. All three are written as legitimate. |
| **Relationship to Rhen** | Friend, bookie and drinking partner. He bets on Rhen in every arena and pays out even when he loses. |
| **Defining line** | *"Coin before cause, cause before crown. And crew before coin, but don't tell the crew."* |

### 2.8 Neve (`char.neve`)

| | |
|---|---|
| **Background** | Raised in the Wick House, a canal orphanage in Lanternhold, alongside Tamsin. She bought her first Guild lantern at fifteen with money she won't explain. She smuggles cargo out of the Reeds, and the cargo is children. |
| **Goal** | To get rich enough that she'll never be anyone's collateral. Secretly, to buy back Tamsin's shadow. |
| **Fear** | Being owned, whether by debt, by the Guild, or by love. |
| **Arc** | From a smuggler with a price for everything to someone who pays for one thing and expects nothing back. **The Last Lantern** (`quest.neve.05`): she takes the Guild seat or burns the shadow ledgers. |
| **Relationship to Rhen** | A business partner who keeps quietly adjusting his tab downward. |
| **Defining line** | *"Everything has a price. The trick is being the one who writes it down."* |

---

## 3. Deep profiles: ten key characters

### 3.1 Emperor Aurem (`boss.aurem`)

| | |
|---|---|
| **Goal** | To hold the seal forever, so the Mourntide never comes back. |
| **Fear** | The dark, literally: his own shadow, and dying and having to face what he cut away. |
| **Arc** | Hero of the Swearing, then deathless emperor, then a man who has spent five hundred years trying not to look down at the floor of his own throne room. In the fight he wields the stones Rhen restored. In Phase III the glass floor cracks, and we see him terrified of the dark for the first time. |
| **Relationship to Rhen** | Rhen is what Aurem could have been: a man who kept his shadow. Aurem offers him the Throne, sincerely, because he wants to rest. |
| **Defining line** | *"I did not steal their shadows, boy. I held them. Do you know how heavy five hundred years of holding is?"* |

### 3.2 The First Shadow (`boss.firstshadow`)

| | |
|---|---|
| **Goal** | To be taken back, and to stop holding. |
| **Fear** | It has no fear left. It is made of fear. |
| **Arc** | Keystone, then Umbra Prime, then rewoven. It tries to sever Rhen from Sable, because a shadow that chose to stay is something it has never been able to understand. When Liss speaks the rewoven clause it shrinks to the shadow of one man and kneels. |
| **Relationship to Rhen** | A dark mirror of Sable, and the fate Sable fears. |
| **Defining line** | *"He cut me away so he could stand in the light. I have been standing in his place ever since."* |

### 3.3 General Hask Varrow (`boss.hask`)

| | |
|---|---|
| **Goal** | Hold the wall until the Oath is repaired. |
| **Fear** | That duty was only ever his excuse never to choose. |
| **Arc** | He signed Rhen's death writ. In the Vow Hall he admits he hoped Rhen would refuse, because Hask never could. At the end he asks Rhen to guide the blade himself. |
| **Relationship to Rhen** | Mentor and father. He taught Rhen to hold a sword, and his Oathwarden hymn is where Rhen's musical theme comes from. |
| **Defining line** | *"I taught you to follow orders, boy. I never taught you to refuse them. Where did you learn that?"* |

### 3.4 Tamsin (`boss.tamsin`)

| | |
|---|---|
| **Goal** | Pay off her debts (her shade-note to Hollis Crane), finish the contract, and survive. |
| **Fear** | Disappearing. Her shadow is fading, and she is the last of the Nine Knives. |
| **Arc** | Hunter, then reluctant ally (the glass-storm), then the last duel on the Tiles. Across six encounters she learns Rhen's habits and remembers them (see `05-Bosses.md`). If spared, she gives him the ninth knife, leads him to Corvaine, and in the epilogue may run the Wick House with Neve. |
| **Relationship to Rhen** | Rival and mirror. They are two people who keep count, of debts in her case and oaths in his. |
| **Defining line** | *"Nine knives, nine debts. You'd be the tenth, and I don't carry ten."* |

### 3.5 Abbot Kessh (`boss.kessh`)

| | |
|---|---|
| **Goal** | To tithe perfectly: every soul to the seal, and none wasted. |
| **Fear** | That the ash in his censers is not holy. |
| **Arc** | He performed thirty thousand rites and presided over Rhen's. In Phase III he drinks the ash itself. In his execution the censers break, he sees the shadows inside them, and in his last breath he understands. |
| **Relationship to Rhen** | Rhen's first true enemy, and the first person to kill him. |
| **Defining line** | *"Kneel, child. It is only fire, and fire is only love that has decided."* |

### 3.6 Mother Ilvane (`boss.ilvane`)

| | |
|---|---|
| **Goal** | To spare children the tithe by drowning them into her daughters' choir. |
| **Fear** | Silence: the day the choir stops singing. |
| **Arc** | A grieving mother who turned her grief into a system of deaths. In her execution she hears her daughters, reaches for them, and sinks toward the voices. The raid `raid.drownedchoir` finishes her story. |
| **Relationship to Rhen** | At first she sees another Choir butcher. By Phase III she sees a parent like herself, one who chose differently. |
| **Defining line** | *"The water keeps them, Oathwarden. The stone only counts them."* |

### 3.7 Archivist Quill (`boss.quill`)

| | |
|---|---|
| **Goal** | To preserve everything, and to keep the secret, because revealing it would unmake the world. |
| **Fear** | Forgetting. The Archive has already taken his own name. |
| **Arc** | He is the gatekeeper who shows Rhen the hidden clause (`mission.sunkenarchive.09`) because he has waited seventy years for someone to read it. Then he fights to keep the page, because he cannot let go of anything. In his execution he hears his forgotten name. |
| **Relationship to Rhen** | Mireth's teacher, and the one who tests Rhen as a reader. |
| **Defining line** | *"Knowing is not the same as remembering, child. I know everything. I remember nothing."* |

### 3.8 Warden-Captain Idris Hale (`char.idris`)

| | |
|---|---|
| **Goal** | Bring Rhen in and restore the Oathwardens' honor. |
| **Fear** | That Rhen was right. |
| **Arc** | Arrests Rhen (`emberfall.01`), lets him pass for one breath (`emberfall.09`), duels him (`bloodmoon.02`), and faces him on the Oathwarden Bridge (`bloodmoon.08`). If spared, he may become the Keeper of Duty and weigh his orders for the first time. If killed, his belt lantern is lit. |
| **Relationship to Rhen** | Sworn brother. They were knighted on the same day. |
| **Defining line** | *"You didn't just break your oath, Rhen. You broke mine."* |

### 3.9 Lady Corvaine (`boss.corvaine`)

| | |
|---|---|
| **Goal** | To see Veloran pay for the murder she was executed for, and to see Tamsin free. |
| **Fear** | That she never loved anyone. That she taught nine children to die. |
| **Arc** | She waits in the Mourning House among the portraits of eight husbands. When Rhen brings the ninth knife she learns Tamsin's fate, and whichever way it went, she lays down her fans. |
| **Relationship to Rhen** | The teacher of his rival. For her, Rhen is the one who carries the last knife home. |
| **Defining line** | *"I buried eight husbands, darling. The ninth buried me. One does learn to dress for it."* |

### 3.10 Ysolde & Yrrah (`boss.petals`)

| | |
|---|---|
| **Goal** | Ysolde: never lose her sister. Yrrah: to be allowed to go. |
| **Fear** | Ysolde fears being alone. Yrrah fears being held forever. |
| **Arc** | A binding that becomes a cut. In Phase II Yrrah speaks to Sable, one bound shadow to another. The Oath choice decides whether Yrrah is tithed (Restore) or freed as a Remnant (Sunder). |
| **Relationship to Rhen** | A mirror of Rhen and Sable: a living person bound to a shadow. What Rhen sees in them is the argument for letting go. |
| **Defining line** | Ysolde: *"We were born holding hands."* Yrrah, a beat later: *"Let go."* |
