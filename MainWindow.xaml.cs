using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace AudioPlayerApp
{
    public partial class MainWindow : Window
    {
        private List<string> audioFiles = new List<string>();
        private int currentTrackIndex = 0;
        private bool isPlaying = false;
        private bool isRepeatEnabled = false;
        private bool isShuffleEnabled = false;
        private bool isDraggingSlider = false;
        private Random random = new Random();
        private DispatcherTimer positionTimer = new DispatcherTimer();
        private DispatcherTimer durationTimer = new DispatcherTimer();
        private string selectedFolderPath;

        public MainWindow()
        {
            InitializeComponent();
            positionTimer.Interval = TimeSpan.FromSeconds(1);
            positionTimer.Tick += PositionTimer_Tick;
            durationTimer.Interval = TimeSpan.FromSeconds(1);
            durationTimer.Tick += DurationTimer_Tick;

        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio files (*.mp3, *.wav, *.m4a)|*.mp3;*.wav;*.m4a";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFolderPath = Path.GetDirectoryName(openFileDialog.FileName);
                audioFiles = openFileDialog.FileNames.ToList();
                currentTrackIndex = 0;
                PlayCurrentTrack();
            }
        }

        private void PlayCurrentTrack()
        {
            if (audioFiles.Count > 0)
            {
                mediaElement.Source = new Uri(audioFiles[currentTrackIndex]);
                mediaElement.Play();
                isPlaying = true;
                positionTimer.Start();
                durationTimer.Start();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isRepeatEnabled)
            {
                mediaElement.Position = TimeSpan.Zero;
                mediaElement.Play();
            }
            else
            {
                NextTrack();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextTrack();
        }

        private void NextTrack()
        {
            if (isShuffleEnabled)
            {
                currentTrackIndex = random.Next(audioFiles.Count);
            }
            else
            {
                currentTrackIndex = (currentTrackIndex + 1) % audioFiles.Count;
            }
            PlayCurrentTrack();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Position.TotalSeconds > 3)
            {
                mediaElement.Position = TimeSpan.Zero;
            }
            else
            {
                if (currentTrackIndex > 0)
                {
                    currentTrackIndex--;
                }
                else
                {
                    currentTrackIndex = audioFiles.Count - 1;
                }
                PlayCurrentTrack();
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mediaElement.Pause();
                isPlaying = false;
            }
            else
            {
                mediaElement.Play();
                isPlaying = true;
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            isRepeatEnabled = !isRepeatEnabled;
            RepeatButton.Content = isRepeatEnabled ? "Повтор: дА!" : "Повтор: нёу";
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            isShuffleEnabled = !isShuffleEnabled;
            ShuffleButton.Content = isShuffleEnabled ? "Перемешка: ага" : "Перемешка: неа)";
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isDraggingSlider)
            {
                mediaElement.Position = TimeSpan.FromSeconds(positionSlider.Value);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            durationTimer.Start();
            positionSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            positionSlider.IsEnabled = true;
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            positionSlider.Value = mediaElement.Position.TotalSeconds;
        }

        private void DurationTimer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan totalTime = mediaElement.NaturalDuration.TimeSpan;
                TimeSpan currentTime = mediaElement.Position;
                string timeRemaining = (totalTime - currentTime).ToString(@"mm\:ss");
                durationTextBlock.Text = timeRemaining;
                positionSlider.Value = currentTime.TotalSeconds;
            }
        }

        private void PositionSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void PositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDraggingSlider = false;
            mediaElement.Position = TimeSpan.FromSeconds(positionSlider.Value);
        }

        private void PlaylistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistListBox.SelectedItem != null)
            {
                string selectedSong = PlaylistListBox.SelectedItem.ToString();
                string filePath = Path.Combine(selectedFolderPath, selectedSong); // Путь к выбранной песне
                mediaElement.Source = new Uri(filePath);
                mediaElement.Play();
            }
        }
    }
}
