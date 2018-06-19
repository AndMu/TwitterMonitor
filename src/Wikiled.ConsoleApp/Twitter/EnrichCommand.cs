using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using NLog;
using Tweetinvi;
using Wikiled.Arff.Persistence;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Twitter
{
    /// <summary>
    /// Enrich -Out=market.csv -Topics=$MMM,$ABT,$ABBV,$ACN,$ATVI,$AYI,$ADBE,$AAP,$AES,$AET,$AMG,$AFL,$A,$APD,$AKAM,$ALK,$ALB,$ALXN,$ALLE,$AGN,$ADS,$LNT,$ALL,$GOOGL,$GOOG,$MO,$AMZN,$AEE,$AAL,$AEP,$AXP,$AIG,$AMT,$AWK,$AMP,$ABC,$AME,$AMGN,$APH,$APC,$ADI,$ANTM,$AON,$APA,$AIV,$AAPL,$AMAT,$ADM,$ARNC,$AJG,$AIZ,$T,$ADSK,$ADP,$AN,$AZO,$AVB,$AVY,$BHI,$BLL,$BAC,$BCR,$BAX,$BBT,$BDX,$BBBY,$BRK.B,$BBY,$BIIB,$BLK,$HRB,$BA,$BWA,$BXP,$BSX,$BMY,$AVGO,$BF.B,$CHRW,$CA,$COG,$CPB,$COF,$CAH,$KMX,$CCL,$CAT,$CBOE,$CBG,$CBS,$CELG,$CNC,$CNP,$CTL,$CERN,$CF,$SCHW,$CHTR,$CHK,$CVX,$CMG,$CB,$CHD,$CI,$XEC,$CINF,$CTAS,$CSCO,$C,$CFG,$CTXS,$CME,$CMS,$COH,$KO,$CTSH,$CL,$CMCSA,$CMA,$CAG,$CXO,$COP,$ED,$STZ,$GLW,$COST,$COTY,$CCI,$CSRA,$CSX,$CMI,$CVS,$DHI,$DHR,$DRI,$DVA,$DE,$DLPH,$DAL,$XRAY,$DVN,$DLR,$DFS,$DISCA,$DISCK,$DG,$DLTR,$D,$DOV,$DOW,$DPS,$DTE,$DD,$DUK,$DNB,$ETFC,$EMN,$ETN,$EBAY,$ECL,$EIX,$EW,$EA,$EMR,$ETR,$EVHC,$EOG,$EQT,$EFX,$EQIX,$EQR,$ESS,$EL,$ES,$EXC,$EXPE,$EXPD,$ESRX,$EXR,$XOM,$FFIV,$FB,$FAST,$FRT,$FDX,$FIS,$FITB,$FSLR,$FE,$FISV,$FLIR,$FLS,$FLR,$FMC,$FTI,$FL,$F,$FTV,$FBHS,$BEN,$FCX,$FTR,$GPS,$GRMN,$GD,$GE,$GGP,$GIS,$GM,$GPC,$GILD,$GPN,$GS,$GT,$GWW,$HAL,$HBI,$HOG,$HAR,$HRS,$HIG,$HAS,$HCA,$HCP,$HP,$HSIC,$HES,$HPE,$HOLX,$HD,$HON,$HRL,$HST,$HPQ,$HUM,$HBAN,$IDXX,$ITW,$ILMN,$INCY,$IR,$INTC,$ICE,$IBM,$IP,$IPG,$IFF,$INTU,$ISRG,$IVZ,$IRM,$JBHT,$JEC,$SJM,$JNJ,$JCI,$JPM,$JNPR,$KSU,$K,$KEY,$KMB,$KIM,$KMI,$KLAC,$KSS,$KHC,$KR,$LB,$LLL,$LH,$LRCX,$LEG,$LEN,$LUK,$LVLT,$LLY,$LNC,$LLTC,$LKQ,$LMT,$L,$LOW,$LYB,$MTB,$MAC,$M,$MNK,$MRO,$MPC,$MAR,$MMC,$MLM,$MAS,$MA,$MAT,$MKC,$MCD,$MCK,$MJN,$MDT,$MRK,$MET,$MTD,$KORS,$MCHP,$MU,$MSFT,$MAA,$MHK,$TAP,$MDLZ,$MON,$MNST,$MCO,$MS,$MSI,$MUR,$MYL,$NDAQ,$NOV,$NAVI,$NTAP,$NFLX,$NWL,$NFX,$NEM,$NWSA,$NWS,$NEE,$NLSN,$NKE,$NI,$NBL,$JWN,$NSC,$NTRS,$NOC,$NRG,$NUE,$NVDA,$ORLY,$OXY,$OMC,$OKE,$ORCL,$PCAR,$PH,$PDCO,$PAYX,$PYPL,$PNR,$PBCT,$PEP,$PKI,$PRGO,$PFE,$PCG,$PM,$PSX,$PNW,$PXD,$PNC,$RL,$PPG,$PPL,$PX,$PCLN,$PFG,$PG,$PGR,$PLD,$PRU,$PEG,$PSA,$PHM,$PVH,$QRVO,$QCOM,$PWR,$DGX,$RRC,$RTN,$O,$RHT,$REG,$REGN,$RF,$RSG,$RAI,$RHI,$ROK,$COL,$ROP,$ROST,$RCL,$R,$SPGI,$CRM,$SCG,$SLB,$SNI,$STX,$SEE,$SRE,$SHW,$SIG,$SPG,$SWKS,$SLG,$SNA,$SO,$LUV,$SWN,$SWK,$SPLS,$SBUX,$STT,$SRCL,$SYK,$STI,$SYMC,$SYF,$SYY,$TROW,$TGT,$TEL,$TGNA,$TDC,$TSO,$TXN,$TXT,$BK,$CLX,$COO,$HSY,$MOS,$TRV,$DIS,$TMO,$TIF,$TWX,$TJX,$TMK,$TSS,$TSCO,$TDG,$RIG,$TRIP,$FOXA,$FOX,$TSN,$USB,$UDR,$ULTA,$UA,$UAA,$UNP,$UAL,$UNH,$UPS,$URI,$UTX,$UHS,$UNM,$URBN,$VFC,$VLO,$VAR,$VTR,$VRSN,$VRSK,$VZ,$VRTX,$VIAB,$V,$VNO,$VMC,$WMT,$WBA,$WM,$WAT,$WEC,$WFC,$HCN,$WDC,$WU,$WRK,$WY,$WHR,$WFM,$WMB,$WLTW,$WYN,$WYNN,$XEL,$XRX,$XLNX,$XL,$XYL,$YHOO,$YUM,$ZBH,$ZION,$ZTS
    /// </summary>
    public class EnrichCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private string[] positive;

        private string[] negative;

        [Required]
        public string Topics { get; set; }

        [Required]
        public string Out { get; set; }

        protected override Task Execute(CancellationToken token)
        {
            log.Info("Starting twitter monitoring...");
            SetupWords();
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            var cleanup = new MessageCleanup();
            var monitor = new PerformanceMonitor(100000);
            var auth = new PersistedAuthentication(new PinConsoleAuthentication());
            var cred = auth.Authenticate(Credentials.Instance.IphoneTwitterCredentials);
            using (Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(item => log.Info(monitor)))
            using (var streamWriter = new StreamWriter(Out, true, new UTF8Encoding(false)))
            using (var csvDataTarget = new CsvWriter(streamWriter))
            {
                Auth.ExecuteOperationWithCredentials(
                    cred,
                    () =>
                        {
                            var enrichments = Enrichment().ToArray();
                            foreach (var enrichment in enrichments)
                            {
                                enrichment.Discovery.BatchSize = 5;
                                enrichment.Discovery.AddProcessed(enrichments.SelectMany(p => p.Discovery.Processed).ToArray());
                                enrichment.Discovery.Process()
                                          .ToObservable()
                                          .ObserveOn(TaskPoolScheduler.Default)
                                          .Select(
                                              x =>
                                                  {
                                                      var text = cleanup.Cleanup(x.Message.Text).Replace("\r\n", " ");
                                                      if (!CanInclude(text, enrichment.Type))
                                                      {
                                                          return x;
                                                      }

                                                      text = Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
                                                      csvDataTarget.WriteField(x.Message.Id);
                                                      csvDataTarget.WriteField(x.Topic);
                                                      csvDataTarget.WriteField(enrichment.Type);
                                                      csvDataTarget.WriteField(text);
                                                      csvDataTarget.NextRecord();
                                                      streamWriter.Flush();
                                                      monitor.Increment();
                                                      return x;
                                                  })
                                          .Wait();
                            }
                        });
            }

            return Task.CompletedTask;
        }

        private bool CanInclude(string text, PositivityType type)
        {
            if (type == PositivityType.Positive)
            {
                return negative.All(word => text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0);
            }

            foreach (var word in positive)
            {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<SentimentDiscovery> Enrichment()
        {
            string[] keywords = string.IsNullOrEmpty(Topics) ? new string[] { } : Topics.Split(',');
            if (Topics.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            yield return new SentimentDiscovery(PositivityType.Negative, new MessageDiscovery(keywords, negative));
            yield return new SentimentDiscovery(PositivityType.Positive, new MessageDiscovery(keywords, positive));
        }

        private void SetupWords()
        {
            Emoji[] happy =
                {
                    Emoji.FACE_WITH_TEARS_OF_JOY,
                    Emoji.SMILING_FACE_WITH_HALO,
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
                    Emoji.HEAVY_HEART_EXCLAMATION_MARK_ORNAMENT,
                    Emoji.COUPLE_WITH_HEART,
                    Emoji.BEATING_HEART,
                    Emoji.TWO_HEARTS,
                    Emoji.SPARKLING_HEART,
                    Emoji.HEART_WITH_ARROW,
                    Emoji.BEATING_HEART,
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
                    Emoji.SMILING_FACE_WITH_HALO,
                    Emoji.VICTORY_HAND
                };

            Emoji[] sad =
                {
                    Emoji.BROKEN_HEART,
                    Emoji.CRYING_CAT_FACE,
                    Emoji.CRYING_FACE,
                    Emoji.WEARY_FACE,
                    Emoji.SLIGHTLY_FROWNING_FACE,
                    Emoji.LOUDLY_CRYING_FACE,
                    Emoji.DISAPPOINTED_FACE,
                    Emoji.FACE_WITH_HEAD_BANDAGE,
                    Emoji.WORRIED_FACE,
                    Emoji.ANGRY_FACE,
                    Emoji.FACE_WITH_NO_GOOD_GESTURE,
                    Emoji.REVERSED_HAND_WITH_MIDDLE_FINGER_EXTENDED,
                    Emoji.UNAMUSED_FACE,
                    Emoji.PILE_OF_POO,
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

            positive = happy.Select(item => Char.ConvertFromUtf32(int.Parse(item.Unified, System.Globalization.NumberStyles.HexNumber))).ToArray();
            negative = sad.Select(item => Char.ConvertFromUtf32(int.Parse(item.Unified, System.Globalization.NumberStyles.HexNumber))).ToArray();
        }
    }
}
