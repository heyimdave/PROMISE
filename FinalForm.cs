﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using System.Diagnostics;
using System.IO;

namespace Final_Kinect
{
    public partial class FinalForm : Form
    {
        // Initializing Kinect Sensor
        KinectSensor mKinectSensor = null;

        // Body frame reader used to detect body by kinect
        MultiSourceFrameReader mBodyframeReader = null;

        Body[] mBody = null;

        // TO be used as basis for visual comparison
        Body[] mBodyOriginal = null;

        // This is the form on which the participant will view the movie.
        SubjectMovieForm mSubjectMovieForm;

        Stopwatch mSessionStopwatch = new Stopwatch();
        Stopwatch mConditionStopwatch = new Stopwatch();

        // This is being used to determine if session is started or stopped, but in some ways that I need to look further into.
        int mSessionState = 0;

        StreamWriter mDataFile;

        int mShoulderRight = 0,
            mShoulderLeft = 0,
            mSpineMid = 0,
            mNeckLeft = 0,
            mNeckRight = 0,
            mSpineShoulder = 0;

        double mShoulderRightLength,
            mShoulderLeftLength,
            mSpineMidLength,
            mNeckLeftLength,
            mNeckRightLength,
            mSpineShoulderLength;

        bool mOriginalAnglesSet = false;

        int mShouderRightMeanSum = 0,
            mShoulderLeftMeanSum = 0,
            mSpineMidMeanSum = 0,
            mNeckMeanSum = 0,
            mNeck1MeanSum = 0,
            mSpineShoulderMeanSum = 0;

        double mHeadPositionDiffMeanSum = 0,
            mNeckPositionDiffMeanSum = 0,
            mSpineBasePositionDiffMeanSum = 0,
            mSpineMidPositionDiffMeanSum = 0,
            mSpineShoulderPositionDiffMeanSum = 0,
            mShoulderLeftPositionDiffMeanSum = 0,
            mShoulderRightPositionDiffMeanSum = 0,
            mElbowLeftPositionDiffMeanSum = 0,
            mElbowRightPositionDiffMeanSum = 0;

        double mHeadPositionDiffMean = 0,
            mNeckPositionDiffMean = 0,
            mSpineBasePositionDiffMean = 0,
            mSpineMidPositionDiffMean = 0,
            mSpineShoulderPositionDiffMean = 0,
            mShoulderLeftPositionDiffMean = 0,
            mShoulderRightPositionDiffMean = 0,
            mElbowLeftPositionDiffMean = 0,
            mElbowRightPositionDiffMean = 0;

        int mOriginalShoulderRight = 0,
            mOriginalShoulderLeft = 0,
            mOriginalSpineMid = 0,
            mOriginalNeckLeft = 0,
            mOriginalNeckRight = 0,
            mOriginalSpineShoulder = 0;

        double mOriginalShoulderRightLength,
            mOriginalShoulderLeftLength,
            mOriginalSpineMidLength,
            mOriginalNeckLeftLength,
            mOriginalNeckRightLength,
            mOriginalSpineShoulderLength;

        CameraSpacePoint mCurrentHeadSpacePoint,
            mCurrentNeckSpacePoint,
            mCurrentSpineBaseSpacePoint,
            mCurrentSpineMidSpacePoint,
            mCurrentSpineShoulderSpacePoint,
            mCurrentShoulderLeftSpacePoint,
            mCurrentShoulderRightSpacePoint,
            mCurrentElbowLeftSpacePoint,
            mCurrentElbowRightSpacePoint;

        CameraSpacePoint mOriginalHeadSpacePoint,
            mOriginalNeckSpacePoint,
            mOriginalSpineBaseSpacePoint,
            mOriginalSpineMidSpacePoint,
            mOriginalSpineShoulderSpacePoint,
            mOriginalShoulderLeftSpacePoint,
            mOriginalShoulderRightSpacePoint,
            mOriginalElbowLeftSpacePoint,
            mOriginalElbowRightSpacePoint;

        double mHeadPositionDiff = 0,
            mNeckPositionDiff = 0,
            mSpineBasePositionDiff = 0,
            mSpineMidPositionDiff = 0,
            mSpineShoulderPositionDiff = 0,
            mShoulderLeftPositionDiff = 0,
            mShoulderRightPositionDiff = 0,
            mElbowLeftPositionDiff = 0,
            mElbowRightPositionDiff = 0;

        int mSmoothingKernal,
            mWarning,
            mNotAllowed;

        int mShoulderRightMean,
            mShoulderLeftMean,
            mSpineMidMean,
            mNeckLeftMean,
            mNeckRightMean,
            mSpineShoulderMean,
            mMovementLowerLimitAgainstOriginal;

        // This should probably be set to zero and iterate earlier down below (if we ultimately need this at all).
        int mMeanDataReadIteration = 1;

        // Will store values in array, and then will find median of the array.
        int[] mShoulderRightMedianArray,
            mShoulderLeftMedianArray,
            mSpineMidMedianArray,
            mNeckMedianArray,
            mNeck1MedianArray,
            mSpineShoulderMedianArray;

        // Will store the median, which will be obtained with the help of the MedianArrays.
        int mShoulderRightMedian,
            mShoulderLeftMedian,
            mSpineMidMedian,
            mNeckLeftMedian,
            mNeckRightMedian,
            mSpineShoulderMedian;

        private void bodyPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;

            // Draw original body (bodies)
            if (mBodyOriginal != null)
            {
                foreach (Body body in mBodyOriginal)
                {
                    if (body.IsTracked)
                    {
                        Helpers.DrawSkeleton(bodyPictureBox, body, graphics);
                    }
                }
            }

            if (mBody != null)
            {
                foreach (Body body in mBody)
                {
                    if (body.IsTracked)
                    {
                        Helpers.DrawSkeleton(bodyPictureBox, body, graphics);
                    }
                }
            }
        }

        /* This will be incremented on every tick. When the tick interval
         * is set for 1000, then mTickCount % 60 will be 0 and thus allow
         * us to perform an action every 1 second - specifically, increment
         * the progress bar.
         */
        int mTickCount = 0;

        Graphics mPictureBoxGraphics;


        private void instructionButton_Click(object sender, EventArgs e)
        {
            UpdateLimitsTextBox("1000", "1000");
        }
        private void noContingencyButton_Click(object sender, EventArgs e)
        {
            UpdateLimitsTextBox("1000", "1000");
        }
        private void movementProbeButton_Click(object sender, EventArgs e)
        {
            UpdateLimitsTextBox("1000", "1000");
        }
        private void shapeButton_Click(object sender, EventArgs e)
        {
            UpdateLimitsTextBox("11", "13");
        }
        private void stepUpButton_Click(object sender, EventArgs e)
        {
            lowerLimitSmallMovementTextBox.Text = (Convert.ToInt32(lowerLimitSmallMovementTextBox.Text) + 2).ToString();
            lowerLimitLargeMovementTextBox.Text = (Convert.ToInt32(lowerLimitLargeMovementTextBox.Text) + 2).ToString();
        }
        private void stepDownButton_Click(object sender, EventArgs e)
        {
            lowerLimitSmallMovementTextBox.Text = (Convert.ToInt32(lowerLimitSmallMovementTextBox.Text) - 2).ToString();
            lowerLimitLargeMovementTextBox.Text = (Convert.ToInt32(lowerLimitLargeMovementTextBox.Text) - 2).ToString();
        }
        private void scanButton_Click(object sender, EventArgs e)
        {
            mOriginalAnglesSet = false;
        }
        private void initializeButton_Click(object sender, EventArgs e)
        {
            InitializeKinect();
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            mSessionState = 22;
            UpdateCurrentLimits(false);

            startButton.BackColor = Color.DeepSkyBlue;
        }
        private void stopButton_Click(object sender, EventArgs e)
        {
            mSessionState = 21;
            startButton.BackColor = Color.Red;
        }
        private void setLimitsButton_Click(object sender, EventArgs e)
        {
            UpdateCurrentLimits(true);
        }

        public FinalForm(
            string subjectInitials,
            string experimentNumber,
            string smoothingKernal,
            string smallMovementLowerLimit,
            string largeMovementLowerLimit,
            string sessionTime,
            int lowerLimitLargeMovement,
            string videoFile,
            string filePath
            )
        {
            InitializeComponent();

            // Obtaining reference to bodyPictureBox graphics object to use for drawing the body.
            mPictureBoxGraphics = bodyPictureBox.CreateGraphics();

            mSmoothingKernal = Convert.ToInt32(smoothingKernal);

            mWarning = Convert.ToInt32(smallMovementLowerLimit);
            mNotAllowed = Convert.ToInt32(largeMovementLowerLimit);

            lowerLimitSmallMovementTextBox.Text = mWarning.ToString();
            lowerLimitLargeMovementTextBox.Text = mNotAllowed.ToString();

            mMovementLowerLimitAgainstOriginal = lowerLimitLargeMovement == 1 ? mNotAllowed : mWarning;

            // Creates int arrays based on the size of the smoothing kernal specified in the SettingsForm.
            mShoulderRightMedianArray = new int[mSmoothingKernal];
            mShoulderLeftMedianArray = new int[mSmoothingKernal];
            mSpineMidMedianArray = new int[mSmoothingKernal];
            mNeckMedianArray = new int[mSmoothingKernal];
            mNeck1MedianArray = new int[mSmoothingKernal];
            mSpineShoulderMedianArray = new int[mSmoothingKernal];

            progressBar.Maximum = (2 * Convert.ToInt32(sessionTime));
            axWindowsMediaPlayer1.URL = videoFile;

            axWindowsMediaPlayer1.Ctlcontrols.stop();

            this.KeyPreview = true;

            startButton.BackColor = Color.Red;

            InitializeDataFiles(filePath, subjectInitials, experimentNumber);

            mSubjectMovieForm = new SubjectMovieForm(videoFile, progressBar.Maximum);
        }

        // Timer event for progress bar
        private void timer1_Tick(object sender, EventArgs e)
        {
            mTickCount++;

            // Every second, increase progress
            if (mTickCount % 60 == 0)
            {
                progressBar.Value++;
                progressBar.Update();

                if (mSubjectMovieForm != null)
                {
                    mSubjectMovieForm.UpdateProgressBar();
                }              
            }
        }
        public void InitializeKinect()
        {
            mKinectSensor = KinectSensor.GetDefault();

            if (mKinectSensor != null)
            {
                // Turn on kinect
                mKinectSensor.Open();

                // We are using kinect camera as well as body detection so here we have used MultiSourceFrameReader
                mBodyframeReader = mKinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
            }

            if (mBodyframeReader != null)
            {
                mBodyframeReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                if (mSubjectMovieForm != null)
                {
                    mSubjectMovieForm.Show();
                }
            }

            if (startButton.Enabled == false)
            {
                startButton.Enabled = true;
            }
            if (stopButton.Enabled == false)
            {
                stopButton.Enabled = true;
            }
        }
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            var reference = e.FrameReference.AcquireFrame();

            // Get body data
            using (var bodyFrame = reference.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (mBody == null)
                    {
                        /* TODO: Should only detect one body and provide message if
                         * more than one body is detected. Then we can not use foreach
                         * below. Also. mBody seems like it can be a local variable.
                         * Also. Is this creating a new body every time? Would that cause
                         * and issue with trying to track fine movements?
                         */
                        mBody = new Body[bodyFrame.BodyFrameSource.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(mBody);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                bodyPictureBox.Refresh();

                foreach (Body body in mBody)
                {
                    // Process if the body has been detected
                    if (body.IsTracked)
                    {
                        if (mSessionState == 22)
                        {
                            startButton.BackColor = Color.DeepSkyBlue;
                        }
                        else if (mSessionState == 21)
                        {
                            startButton.BackColor = Color.Red; // Start button normally starts as red, fyi.
                            MediaPlayersPause();

                            timer1.Enabled = false;
                        }
                        else if (mSessionState == 20) // This is when state was not 22, and then there was too much movement, I believe.
                        {
                            startButton.BackColor = Color.Blue;
                        }
                        else
                        {
                            startButton.BackColor = Color.Green;
                        }

                        // Joints
                        var head = body.Joints[JointType.Head];
                        var neck = body.Joints[JointType.Neck];
                        var spineBase = body.Joints[JointType.SpineBase];
                        var spineMid = body.Joints[JointType.SpineMid];
                        var spineShoulder = body.Joints[JointType.SpineShoulder];
                        var shoulderLeft = body.Joints[JointType.ShoulderLeft];
                        var shoulderRight = body.Joints[JointType.ShoulderRight];
                        var elbowLeft = body.Joints[JointType.ElbowLeft];
                        var elbowRight = body.Joints[JointType.ElbowRight];

                        // Setting current CameraSpacePoints
                        mCurrentHeadSpacePoint = body.Joints[JointType.Head].Position;
                        mCurrentNeckSpacePoint = body.Joints[JointType.Neck].Position;
                        mCurrentSpineBaseSpacePoint = body.Joints[JointType.SpineBase].Position;
                        mCurrentSpineMidSpacePoint = body.Joints[JointType.SpineMid].Position;
                        mCurrentSpineShoulderSpacePoint = body.Joints[JointType.SpineShoulder].Position;
                        mCurrentShoulderLeftSpacePoint = body.Joints[JointType.ShoulderLeft].Position;
                        mCurrentShoulderRightSpacePoint = body.Joints[JointType.ShoulderRight].Position;
                        mCurrentElbowLeftSpacePoint = body.Joints[JointType.ElbowLeft].Position;
                        mCurrentElbowRightSpacePoint = body.Joints[JointType.ElbowRight].Position;

                        // Calculate distance in millimeters between joints. Multiplication is to convert return value of meters
                        mShoulderRightLength = MathExtensions.Length(neck.Position, elbowRight.Position) * 1000;
                        mShoulderLeftLength = MathExtensions.Length(neck.Position, elbowLeft.Position) * 1000;
                        mSpineMidLength = MathExtensions.Length(spineBase.Position, head.Position) * 1000;
                        mNeckLeftLength = MathExtensions.Length(head.Position, shoulderLeft.Position) * 1000;
                        mNeckRightLength = MathExtensions.Length(head.Position, shoulderRight.Position) * 1000;
                        mSpineShoulderLength = MathExtensions.Length(head.Position, spineShoulder.Position) * 1000;

                        // Angles - doubles are returned by Angle(). Yet, later they are cast to int. Is this an issue?
                        var neckToElbowRightAngle = shoulderRight.Angle(neck, elbowRight);
                        var neckToElbowLeftAngle = shoulderLeft.Angle(neck, elbowLeft);
                        var spineBaseToHeadAngle = spineMid.Angle(spineBase, head);
                        var headToShoulderLeftAngle = neck.Angle(head, shoulderLeft);
                        var headToShoulderRightAngle = neck.Angle(head, shoulderRight);
                        var headToSpineShoulder = neck.Angle(head, spineShoulder);

                        mShoulderRight = (int) neckToElbowRightAngle;
                        mShoulderLeft = (int) neckToElbowLeftAngle;
                        mSpineMid = (int) spineBaseToHeadAngle;
                        mNeckLeft = (int) headToShoulderLeftAngle;
                        mNeckRight = (int) headToShoulderRightAngle;
                        mSpineShoulder = (int) headToSpineShoulder;

                        // Once the user has pressed the start button                       
                        if (mSessionState == 22) // and deep sky blue button
                        {
                            if (timer1.Enabled == false)
                            {
                                timer1.Enabled = true;
                            }

                            // Session stopwatch 
                            if (mSessionStopwatch.IsRunning == false)
                            {
                                mSessionStopwatch.Start();
                            }

                            sessionElapsedTimeTextBox.Text = mSessionStopwatch.Elapsed.ToString();

                            // Condition stopwatch
                            if (mConditionStopwatch.IsRunning == false)
                            {
                                mConditionStopwatch.Start();
                            }

                            conditionElapsedTimeTextBox.Text = mConditionStopwatch.Elapsed.ToString();

                            if (!mOriginalAnglesSet)
                            {
                                SetOriginalAngles();
                            }

                            SetPositionDiff();
                            UpdateMedianArrays();
                            UpdateMeanSums();

                            if (mMeanDataReadIteration == mSmoothingKernal)
                            {   
                                MeansUpdates();

                                mMeanDataReadIteration = 0;                               
                            }

                            // mMeanDataReadIteration was reset in the above conditional
                            mMeanDataReadIteration++;
                        }
                        // If not started
                        else if (mSessionState == 21)
                        {
                            /*
                            if (mShoulderRight > (mOriginalShoulderRight - mMovementLowerLimitAgainstOriginal) &&
                                mShoulderRight < (mOriginalShoulderRight + mMovementLowerLimitAgainstOriginal) &&
                                mShoulderLeft > (mOriginalShoulderLeft - mMovementLowerLimitAgainstOriginal) &&
                                mShoulderLeft < (mOriginalShoulderLeft + mMovementLowerLimitAgainstOriginal) &&
                                mSpineMid > (mOriginalSpineMid - mMovementLowerLimitAgainstOriginal) &&
                                mSpineMid < (mOriginalSpineMid + mMovementLowerLimitAgainstOriginal) &&
                                mNeckLeft > (mOriginalNeckLeft - mMovementLowerLimitAgainstOriginal) &&
                                mNeckLeft < (mOriginalNeckLeft + mMovementLowerLimitAgainstOriginal) &&
                                mNeckRight > (mOriginalNeckRight - mMovementLowerLimitAgainstOriginal) &&
                                mNeckRight < (mOriginalNeckRight + mMovementLowerLimitAgainstOriginal) &&
                                mSpineShoulder > (mOriginalSpineShoulder - mMovementLowerLimitAgainstOriginal) &&
                                mSpineShoulder < (mOriginalSpineShoulder + mMovementLowerLimitAgainstOriginal))*/

                            if (
                                Math.Abs(mHeadPositionDiff) < mNotAllowed &&
                                Math.Abs(mNeckPositionDiff) < mNotAllowed &&
                                Math.Abs(mSpineBasePositionDiff) < mNotAllowed &&
                                Math.Abs(mSpineMidPositionDiff) < mNotAllowed &&
                                Math.Abs(mSpineShoulderPositionDiff) < mNotAllowed &&
                                Math.Abs(mShoulderLeftPositionDiff) < mNotAllowed &&
                                Math.Abs(mShoulderRightPositionDiff) < mNotAllowed &&
                                Math.Abs(mElbowLeftPositionDiff) < mNotAllowed &&
                                Math.Abs(mElbowRightPositionDiff) < mNotAllowed
                            )
                            {
                                mSessionState = 20;
                            }
                        }
                    }
                }
            }
        }

        private void SetOriginalAngles()
        {
            mOriginalHeadSpacePoint = mCurrentHeadSpacePoint;
            mOriginalNeckSpacePoint = mCurrentNeckSpacePoint;
            mOriginalSpineBaseSpacePoint = mCurrentSpineBaseSpacePoint;
            mOriginalSpineMidSpacePoint = mCurrentSpineMidSpacePoint;
            mOriginalSpineShoulderSpacePoint = mCurrentSpineShoulderSpacePoint;
            mOriginalShoulderLeftSpacePoint = mCurrentShoulderLeftSpacePoint;
            mOriginalShoulderRightSpacePoint = mCurrentShoulderRightSpacePoint;
            mOriginalElbowLeftSpacePoint = mCurrentElbowLeftSpacePoint;
            mOriginalElbowRightSpacePoint = mCurrentElbowRightSpacePoint;
           
            mOriginalShoulderLeft = mShoulderLeft;
            mOriginalShoulderRight = mShoulderRight;
            mOriginalSpineMid = mSpineMid;
            mOriginalNeckLeft = mNeckLeft;
            mOriginalNeckRight = mNeckRight;
            mOriginalSpineShoulder = mSpineShoulder;

            mOriginalShoulderRightLength = mShoulderLeftLength;
            mOriginalShoulderLeftLength = mShoulderRightLength;
            mOriginalSpineMidLength = mSpineMidLength;
            mOriginalNeckLeftLength = mNeckLeftLength;
            mOriginalNeckRightLength = mNeckRightLength;
            mOriginalSpineShoulderLength = mSpineShoulderLength;

            mOriginalAnglesSet = true;

            mDataFile.WriteLine("SCAN," + mSessionStopwatch.Elapsed.ToString());
        }

        private int GetMedian(int[] medianSmoothing)
        {
            int tmp;

            for (int i = 0; i < mSmoothingKernal; i++)
            {
                for (int j = i + 1; j < mSmoothingKernal; j++)
                {
                    if (medianSmoothing[j] < medianSmoothing[i])
                    {
                        tmp = medianSmoothing[i];
                        medianSmoothing[i] = medianSmoothing[j];
                        medianSmoothing[j] = tmp;
                    }
                }
            }

            if (mSmoothingKernal % 2 == 0)
            {
                return (medianSmoothing[mSmoothingKernal / 2] + medianSmoothing[mSmoothingKernal / 2 + 1]) / 2;
            }
            else
            {
                return medianSmoothing[(mSmoothingKernal + 1) / 2];
            }
        }
        private void UpdateMedianArrays()
        {
            int element = mMeanDataReadIteration - 1; // Perhaps using a parameter would be better than mMeanDataReadIteration.

            mShoulderRightMedianArray[element] = mShoulderRight;
            mShoulderLeftMedianArray[element] = mShoulderLeft;
            mSpineMidMedianArray[element] = mSpineMid;
            mNeckMedianArray[element] = mNeckLeft;
            mNeck1MedianArray[element] = mNeckRight;
            mSpineShoulderMedianArray[element] = mSpineShoulder;
        }

        private void UpdateMeanSums()
        {/*
            mShouderRightMeanSum += mShoulderRight;
            mShoulderLeftMeanSum += mShoulderLeft;
            mSpineMidMeanSum += mSpineMid;
            mNeckMeanSum += mNeckLeft;
            mNeck1MeanSum += mNeckRight;
            mSpineShoulderMeanSum += mSpineShoulder;
            */
            mHeadPositionDiffMeanSum += mHeadPositionDiff;
            mNeckPositionDiffMeanSum += mNeckPositionDiff;
            mSpineBasePositionDiffMeanSum += mSpineBasePositionDiff;
            mSpineMidPositionDiffMeanSum += mSpineMidPositionDiff;
            mSpineShoulderPositionDiffMeanSum += mSpineShoulderPositionDiff;
            mShoulderLeftPositionDiffMeanSum += mShoulderLeftPositionDiff;
            mShoulderRightPositionDiffMeanSum += mShoulderRightPositionDiff;
            mElbowLeftPositionDiffMeanSum += mElbowLeftPositionDiff;
            mElbowRightPositionDiffMeanSum += mElbowRightPositionDiff;

        }
        private void ResetMeanSums()
        {
            mHeadPositionDiffMeanSum = 0;
            mNeckPositionDiffMeanSum = 0;
            mSpineBasePositionDiffMeanSum = 0;
            mSpineMidPositionDiffMeanSum = 0;
            mSpineShoulderPositionDiffMeanSum = 0;
            mShoulderLeftPositionDiffMeanSum = 0;
            mShoulderRightPositionDiffMeanSum = 0;
            mElbowLeftPositionDiffMeanSum = 0;
            mElbowRightPositionDiffMeanSum = 0;

/*
            mShouderRightMeanSum = 0;
            mShoulderLeftMeanSum = 0;
            mSpineMidMeanSum = 0;
            mNeckMeanSum = 0;
            mNeck1MeanSum = 0;
            mSpineShoulderMeanSum = 0;
            */
        }
        private void SetPositionDiff()
        {
            mHeadPositionDiff = MathExtensions.Length(mCurrentHeadSpacePoint, mOriginalHeadSpacePoint) * 1000;
            mNeckPositionDiff = MathExtensions.Length(mCurrentNeckSpacePoint, mOriginalNeckSpacePoint) * 1000;
            mSpineBasePositionDiff = MathExtensions.Length(mCurrentSpineBaseSpacePoint, mOriginalSpineBaseSpacePoint) * 1000;
            mSpineMidPositionDiff = MathExtensions.Length(mCurrentSpineMidSpacePoint, mOriginalSpineMidSpacePoint) * 1000;
            mSpineShoulderPositionDiff = MathExtensions.Length(mCurrentSpineShoulderSpacePoint, mOriginalSpineShoulderSpacePoint) * 1000;
            mShoulderLeftPositionDiff = MathExtensions.Length(mCurrentShoulderLeftSpacePoint, mOriginalShoulderLeftSpacePoint) * 1000;
            mShoulderRightPositionDiff = MathExtensions.Length(mCurrentShoulderRightSpacePoint, mOriginalShoulderRightSpacePoint) * 1000;
            mElbowLeftPositionDiff = MathExtensions.Length(mCurrentElbowLeftSpacePoint, mOriginalElbowLeftSpacePoint) * 1000;
            mElbowRightPositionDiff = MathExtensions.Length(mCurrentElbowRightSpacePoint, mOriginalElbowRightSpacePoint) * 1000;
        }

        private void SetMeans()
        {
            mHeadPositionDiffMean = mHeadPositionDiffMeanSum / mSmoothingKernal;
            mNeckPositionDiffMean = mNeckPositionDiffMeanSum / mSmoothingKernal;
            mSpineBasePositionDiffMean = mSpineBasePositionDiffMeanSum / mSmoothingKernal;
            mSpineMidPositionDiffMean = mSpineMidPositionDiffMeanSum / mSmoothingKernal;
            mSpineShoulderPositionDiffMean = mSpineShoulderPositionDiffMeanSum / mSmoothingKernal;
            mShoulderLeftPositionDiffMean = mShoulderLeftPositionDiffMeanSum / mSmoothingKernal;
            mShoulderRightPositionDiffMean = mShoulderRightPositionDiffMeanSum / mSmoothingKernal;
            mElbowLeftPositionDiffMean = mElbowLeftPositionDiffMeanSum / mSmoothingKernal;
            mElbowRightPositionDiffMean = mElbowRightPositionDiffMeanSum / mSmoothingKernal;

            /*
            mShoulderRightMean = mShouderRightMeanSum / mSmoothingKernal;
            mShoulderLeftMean = mShoulderLeftMeanSum / mSmoothingKernal;
            mSpineMidMean = mSpineMidMeanSum / mSmoothingKernal;
            mNeckLeftMean = mNeckMeanSum / mSmoothingKernal;
            mNeckRightMean = mNeck1MeanSum / mSmoothingKernal;
            mSpineShoulderMean = mSpineShoulderMeanSum / mSmoothingKernal;
            */
        }
        private void SetMedians()
        {
            mShoulderRightMedian = GetMedian(mShoulderRightMedianArray);
            mShoulderLeftMedian = GetMedian(mShoulderLeftMedianArray);
            mSpineMidMedian = GetMedian(mSpineMidMedianArray);
            mNeckLeftMedian = GetMedian(mNeckMedianArray);
            mNeckRightMedian = GetMedian(mNeck1MedianArray);
            mSpineShoulderMedian = GetMedian(mSpineShoulderMedianArray);
        }

        private void MeanRedLightUpdate() // These update functions can probably be combined into one function
        {
            MediaPlayersPause();

            if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
            }

            SetAllTrafficLights(true, false, false);

            mSessionStopwatch.Stop();
            mConditionStopwatch.Stop();
            mSessionState = 21;

            UpdateDataFile("Large Movement");
        }
        private void MeanYellowLightUpdate()
        {
            if (timer1.Enabled == false)
            {
                timer1.Enabled = true;
            }

            MediaPlayersPlay();

            SetAllTrafficLights(false, true, false);

            UpdateDataFile("Small Movement");
        }
        private void MeanGreenLightUpdate()
        {
            if (timer1.Enabled == false)
            {
                timer1.Enabled = true;
            }

            MediaPlayersPlay();
                              
            SetAllTrafficLights(false, false, true);

            UpdateDataFile("");
        }

        private bool MeanOver(int limit)
        {
            return (
                Math.Abs(MathExtensions.Length(mCurrentHeadSpacePoint, mOriginalHeadSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentNeckSpacePoint, mOriginalNeckSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentSpineBaseSpacePoint, mOriginalSpineBaseSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentSpineMidSpacePoint, mOriginalSpineMidSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentSpineShoulderSpacePoint, mOriginalSpineShoulderSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentShoulderLeftSpacePoint, mOriginalShoulderLeftSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentShoulderRightSpacePoint, mOriginalShoulderRightSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentElbowLeftSpacePoint, mOriginalElbowLeftSpacePoint) * 1000) > limit ||
                Math.Abs(MathExtensions.Length(mCurrentElbowRightSpacePoint, mOriginalElbowRightSpacePoint) * 1000) > limit
            );
        }
        private void MeansUpdates()
        {
            SetMeans();
            SetMedians();

            if (MeanOver(mNotAllowed))
            {
                MeanRedLightUpdate();
            }
            else if (MeanOver(mWarning))
            {
                MeanYellowLightUpdate();
            }
            else
            {
                MeanGreenLightUpdate();
            }

            ResetMeanSums();
        }

        private void UpdateLimitsTextBox(String warning, String notAllowed)
        {
            lowerLimitSmallMovementTextBox.Text = warning;
            lowerLimitLargeMovementTextBox.Text = notAllowed;
        }
        private void UpdateCurrentLimits(bool updateDateFile)
        {
            if (updateDateFile == true)
            {
                mDataFile.WriteLine("Contingency Change," + mSessionStopwatch.Elapsed.ToString() + "," + lowerLimitSmallMovementTextBox.Text + "," + lowerLimitLargeMovementTextBox.Text);
            }

            mWarning = Convert.ToInt32(lowerLimitSmallMovementTextBox.Text);
            mNotAllowed = Convert.ToInt32(lowerLimitLargeMovementTextBox.Text);

            mConditionStopwatch.Restart();

            UpdateCurrentLimitsLabel();
        }
        private void UpdateCurrentLimitsLabel()
        {
            currentLimitsLabel.Text = "Current Limits: Warning - " + mWarning.ToString() + "  Not Allowed - " + mNotAllowed.ToString();
        }

        private void InitializeDataFiles(String filePath, String subjectInitials, String experimentNumber)
        {
            mDataFile = new StreamWriter(
                Path.Combine(filePath + "\\" + subjectInitials + "-" + experimentNumber + "-" + DateTime.Now.ToString("yyyyMMdd") + "-" + "data.csv"),
                true
            );

            mDataFile.WriteLine("Subject Initials" + "," + "Experiment No" + "," + "Smoothing Kernel" + "," + "Small Movement" + "," + "Large Movement");
            mDataFile.WriteLine(subjectInitials + "," + experimentNumber + "," + mSmoothingKernal + "," + mWarning + "," + mNotAllowed + "\r\n");

            mDataFile.WriteLine(
                "Event," +
                "Timestamp," +
                "MeanHeadDiff,MeanNeckDiff,MeanSpineMid,MeanSpineBaseDiff,MeanSpineMidDiff,MeanSpineShoulderDiff,MeanShoulderLeftDiff,MeanShoulderRightDiff,MeanElbowLeftDiff,MeanElbowRightDIff,," +
                "MedianShoulderLeft,MedianShoulderRight,MedianSpineMid,MedianNeckLeft,MedianNeckRight,MedianSpineShoulder,," +
                "MedianDiff,,,,,,," +
                "RawShoulderLeft,RawShoulderRight,RawSpineMid,RawNeckLeft,RawNeckRight,RawSpineShoulder,," +
                "RawDiff"
            );
        }
        private void UpdateDataFile(String sessionEvent)
        {
            mDataFile.WriteLine(
                sessionEvent + "," +
                // Mean Diff
                mHeadPositionDiff.ToString() + "," +
                mNeckPositionDiff.ToString() + "," +
                mSpineBasePositionDiff.ToString() + "," +
                mSpineMidPositionDiff.ToString() + "," +
                mSpineShoulderPositionDiff.ToString() + "," +
                mShoulderLeftPositionDiff.ToString() + "," +
                mShoulderRightPositionDiff.ToString() + "," +
                mElbowLeftPositionDiff.ToString() + "," +
                mElbowRightPositionDiff.ToString() + ",," +
                // Median
                mShoulderLeftMedian + "," +
                mShoulderRightMedian + "," +
                mSpineMidMedian + "," +
                mNeckLeftMedian + "," +
                mNeckRightMedian + "," +
                mSpineShoulderMedian + ",," +
                // Median Diff
                (mOriginalShoulderLeft - mShoulderLeftMedian).ToString() + "," +
                (mOriginalShoulderRight - mShoulderRightMedian).ToString() + "," +
                (mOriginalSpineMid - mSpineMidMedian).ToString() + "," +
                (mOriginalNeckLeft - mNeckLeftMedian).ToString() + "," +
                (mOriginalNeckRight - mNeckRightMedian).ToString() + "," +
                (mOriginalSpineShoulder - mSpineShoulderMedian).ToString() + ",," +
                // Raw
                mShoulderLeft + "," +
                mShoulderRight + "," +
                mSpineMid + "," +
                mNeckLeft + "," +
                mNeckRight + "," +
                mSpineShoulder + ",," +
                // Raw Diff
                (mOriginalShoulderLeft - mShoulderLeft).ToString() + "," +
                (mOriginalShoulderRight - mShoulderRight).ToString() + "," +
                (mOriginalSpineMid - mSpineMid).ToString() + "," +
                (mOriginalNeckLeft - mNeckLeft).ToString() + "," +
                (mOriginalNeckRight - mNeckRight).ToString() + "," +
                (mOriginalSpineShoulder - mSpineShoulder).ToString() + ",," +
                // Millimeters
                mShoulderLeftLength + "," +
                mShoulderRightLength + "," +
                mSpineMidLength + "," +
                mNeckLeftLength + "," +
                mNeckRightLength + "," +
                mSpineShoulderLength + ",," +
                // mm Diff
                (mOriginalShoulderLeftLength - mShoulderLeftLength).ToString() + "," +
                (mOriginalShoulderRightLength - mShoulderRightLength).ToString() + "," +
                (mOriginalSpineMidLength - mSpineMidLength).ToString() + "," +
                (mOriginalNeckLeftLength - mNeckLeftLength).ToString() + "," +
                (mOriginalNeckRightLength - mNeckRightLength).ToString() + "," +
                (mOriginalSpineShoulderLength - mSpineShoulderLength).ToString()

            );
        }

        private void MediaPlayersPlay()
        {
            axWindowsMediaPlayer1.Ctlcontrols.play();

            // Play the participant's movie as well
            if (mSubjectMovieForm != null)
            {
                mSubjectMovieForm.MediaPlayerPlay();
            }
        }
        private void MediaPlayersPause()
        {
            axWindowsMediaPlayer1.Ctlcontrols.pause();

            // Pause participant's movie as well.
            if (mSubjectMovieForm != null)
            {
                mSubjectMovieForm.MediaPlayerPause();
            }
        }

        private void SetAllTrafficLights(bool redVisible, bool yellowVisible, bool greenVisible)
        {
            redLightPictureBox.Visible = redVisible;
            yellowLightPictureBox.Visible = yellowVisible;
            greenLightPictureBox.Visible = greenVisible;

            // Change the participant's lights as well
            if (mSubjectMovieForm != null)
            {
                mSubjectMovieForm.SetRedLightVisible(redVisible);
                mSubjectMovieForm.SetYellowLightVisible(yellowVisible);
                mSubjectMovieForm.SetGreenLightVisible(greenVisible);
            }
        }

        private void FinalForm_Load(object sender, EventArgs e)
        {            
            if (mSessionState == 22)
            {
                this.BackColor = System.Drawing.Color.Black;
            }
        }
        private void FinalForm_FormClosing(object sender, FormClosingEventArgs e)
        {            
            // Write notes to the end of the median file
            mDataFile.WriteLine("\r\nNotes:\r\n\r\n" + notesTextBox.Text);

            // Close all data files when form closes
            mDataFile.Close();
            mSubjectMovieForm.Close();

            mPictureBoxGraphics.Dispose();
        }
        private void FinalForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
