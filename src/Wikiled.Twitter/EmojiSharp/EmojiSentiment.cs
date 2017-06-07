using System.Collections.Generic;
using System.Linq;
using EmojiSharp;

namespace Wikiled.Twitter.EmojiSharp
{
    public static class EmojiSentiment
    {
        public static Emoji[] Happy = new[]
                                      {
                                          Emoji.FACE_WITH_TEARS_OF_JOY,
                                          Emoji.CAT_FACE_WITH_TEARS_OF_JOY,
                                          Emoji.SMILING_CAT_FACE_WITH_HEART_SHAPED_EYES,
                                          Emoji.SMILING_CAT_FACE_WITH_OPEN_MOUTH,
                                          Emoji.SMILING_FACE_WITH_HALO,
                                          Emoji.SMILING_FACE_WITH_HEART_SHAPED_EYES,
                                          Emoji.SMILING_FACE_WITH_OPEN_MOUTH,
                                          Emoji.SMILING_FACE_WITH_OPEN_MOUTH_AND_COLD_SWEAT,
                                          Emoji.SMILING_FACE_WITH_OPEN_MOUTH_AND_SMILING_EYES,
                                          Emoji.SMILING_FACE_WITH_OPEN_MOUTH_AND_TIGHTLY_CLOSED_EYES,
                                          Emoji.SMILING_FACE_WITH_SMILING_EYES,
                                          Emoji.SMILING_FACE_WITH_SUNGLASSES,
                                          Emoji.KISSING_FACE_WITH_SMILING_EYES,
                                          Emoji.KISS,
                                          Emoji.KISSING_CAT_FACE_WITH_CLOSED_EYES,
                                          Emoji.KISSING_FACE,
                                          Emoji.BLACK_HEART_SUIT,
                                          Emoji.HEAVY_HEART_EXCLAMATION_MARK_ORNAMENT,
                                          Emoji.COUPLE_WITH_HEART,
                                          Emoji.BEATING_HEART,
                                          Emoji.TWO_HEARTS,
                                          Emoji.SPARKLING_HEART,
                                          Emoji.HEART_WITH_ARROW,
                                          Emoji.HEART_WITH_RIBBON,
                                          Emoji.BEATING_HEART,
                                          Emoji.BLUE_HEART,
                                          Emoji.PURPLE_HEART,
                                          Emoji.REVOLVING_HEARTS,
                                          Emoji.HEART_DECORATION,
                                          Emoji.BABY_ANGEL,
                                          Emoji.FACE_WITH_LOOK_OF_TRIUMPH,
                                          Emoji.SLIGHTLY_SMILING_FACE,
                                          Emoji.HAPPY_PERSON_RAISING_ONE_HAND,
                                          Emoji.PERSON_RAISING_BOTH_HANDS_IN_CELEBRATION,
                                          Emoji.OK_HAND_SIGN,
                                          Emoji.FACE_WITH_OK_GESTURE,
                                          Emoji.CLAPPING_HANDS_SIGN,
                                          Emoji.OPEN_HANDS_SIGN,
                                          Emoji.HUGGING_FACE,
                                          Emoji.WINKING_FACE,
                                          Emoji.FACE_THROWING_A_KISS,
                                          Emoji.WHITE_SMILING_FACE,
                                          Emoji.SMIRKING_FACE,
                                          Emoji.RELIEVED_FACE,
                                          Emoji.FACE_WITH_STUCK_OUT_TONGUE,
                                          Emoji.FACE_WITH_STUCK_OUT_TONGUE_AND_WINKING_EYE,
                                          Emoji.UPSIDE_DOWN_FACE,
                                          Emoji.MONEY_MOUTH_FACE,
                                          Emoji.MONEY_BAG,
                                          Emoji.SMILING_FACE_WITH_HALO, 
                                          Emoji.VICTORY_HAND, 
                                      };

        public static Emoji[] Surprised = new[]
                                 {
                                        Emoji.HUSHED_FACE,
                                        Emoji.FACE_WITH_OPEN_MOUTH,
                                        Emoji.ASTONISHED_FACE,
                                        Emoji.PERSON_WITH_POUTING_FACE,
                                        Emoji.THINKING_FACE
                                    };

        public static Emoji[] Sad = new[]
                                    {
                                        Emoji.BROKEN_HEART,
                                        Emoji.CRYING_CAT_FACE,
                                        Emoji.CRYING_FACE,
                                        Emoji.WEARY_FACE,
                                        Emoji.SLIGHTLY_FROWNING_FACE,
                                        Emoji.LOUDLY_CRYING_FACE,
                                        Emoji.DISAPPOINTED_FACE,
                                        Emoji.FACE_WITH_HEAD_BANDAGE, 
                                    };

        public static Emoji[] Anger = new[]
                                      {
                                          Emoji.ANGRY_FACE,
                                          Emoji.WORRIED_FACE,
                                          Emoji.POUTING_FACE,
                                          Emoji.CAT_FACE_WITH_WRY_SMILE,
                                          Emoji.FACE_WITH_NO_GOOD_GESTURE,
                                          Emoji.REVERSED_HAND_WITH_MIDDLE_FINGER_EXTENDED,
                                          Emoji.UNAMUSED_FACE,
                                          Emoji.IMP, 
                                          Emoji.SKULL, 
                                          Emoji.SKULL_AND_CROSSBONES, 
                                          Emoji.PILE_OF_POO, 
                                      };

        public static Emoji[] Scary = new[]
                                      {
                                          Emoji.PERSEVERING_FACE,
                                          Emoji.ANGUISHED_FACE,
                                          Emoji.FEARFUL_FACE,
                                          Emoji.FACE_SCREAMING_IN_FEAR,
                                          Emoji.FACE_WITH_COLD_SWEAT,
                                          Emoji.FROWNING_FACE_WITH_OPEN_MOUTH,
                                          Emoji.CONFUSED_FACE,
                                          Emoji.WHITE_FROWNING_FACE,
                                          Emoji.CONFOUNDED_FACE,
                                          Emoji.PERSON_FROWNING,
                                          Emoji.GRIMACING_FACE, 
                                          Emoji.FACE_WITH_OPEN_MOUTH_AND_COLD_SWEAT,
                                          Emoji.POUTING_FACE
                                      };

        public static IEnumerable<Emoji> GetPositive()
        {
            return Happy;
        }

        public static IEnumerable<Emoji> GetNegative()
        {
            return Sad.Union(Anger).Union(Scary);
        }
    }
}
