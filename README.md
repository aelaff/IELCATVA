# Integrating Equation-Based Labeling and Classification for Adaptive Turkish Vocabulary Acquisition (IELCATVA)
 
This repository presents a bilingual, hybrid system designed to enhance vocabulary acquisition and assessment for Turkish language learners. It integrates behavioral analysis with linguistic insights through a machine learning-based approach. The system includes a Unity-based mobile application for students and a RESTful API backend that powers adaptive vocabulary evaluation.

---

## 🗂 Repository Structure

.
├── UnityApp/   # Frontend mobile application built with Unity
└── Endpoint/   # Backend APIs for behavioral vocabulary analysis
---

## 📱 UnityApp

This is the mobile-facing component where learners interact with vocabulary exercises and exams. It includes features like:

- Vocabulary quizzes and exercises  
- Real-time tracking of learner progress  
- Highlighting individual vocabulary gaps by category  
- Visualization of behavioral cues (hesitation, retries, etc.)  
- Adaptive learning flow based on backend insights  

To connect the app with the backend, update the `apiUrl` in the following Unity script:  
`Assets/MyProject/Scripts/Controller/APIManager.cs`

---

## 🌐 Endpoint

The `Endpoint` folder contains the RESTful API powered by a machine learning model that:

- Receives learner interaction data
- Analyzes behavioral patterns like:
  - Hesitation count
  - Reaction times
  - Answer attempts
- Predicts vocabulary gaps using a Support Vector Machine (SVM) with an RBF kernel
- Sends classification results back to the Unity app

---

### 🧠 Machine Learning Details

- **Model**: Support Vector Machine (SVM) with RBF Kernel  
- **Library**: [PHP-ML](https://github.com/php-ai/php-ml)  
- **Optimized Parameters**: C = 10, γ = 0.1  
- **Dataset**: 1,000 interactions from 20 learners  
- **Features**: 
  - Behavioral: Hesitations, reaction times, retries  
  - Linguistic: Word difficulty, theme  
- **Performance**:
  - Accuracy: **89%**
  - Precision: **86%**
  - Recall: **91%**
  - F1-Score: **88%**

The RBF kernel captures complex relationships, such as the cumulative effect of hesitation and wrong answers on medium-difficulty words—making it central to the model’s success.

---


## 🔐 Firebase Integration (Auth & Database)

The Unity app uses **Firebase** to handle user authentication and real-time database storage.  
To set it up correctly:

### ✅ Required Packages

Install the following Unity Firebase SDK packages:

- **Firebase Auth**
- **Firebase Realtime Database**
- (Optional but recommended) **Firebase Analytics**

These can be downloaded via [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).

### 🔧 Setup Steps

1. Go to the [Firebase Console](https://console.firebase.google.com/) and create a new project (or use an existing one).

2. Download the configuration files:
   - `google-services.json` for Android
   - `GoogleService-Info.plist` for iOS

3. Add the configuration files to your Unity project:
Assets/ ├── GoogleService-Info.plist # For iOS └── google-services.json # For Android

4. In Unity, go to:
Edit > Project Settings > Player > Other Settings

And make sure `Internet Access` is enabled and `IL2CPP` backend is selected for Android.

5. Open Unity and allow the Firebase dependencies to resolve.

6. Your project is now connected to Firebase for:
- **Authentication**: Email/password-based user login & signup
- **Database**: Syncing student progress, performance, and vocabulary gaps in real time

## ⚙️ Setup & Deployment

### 🔧 Backend (Endpoint)

1. Clone the repo and navigate to the `Endpoint/` folder.

2. Download and install [PHP-ML](https://github.com/php-ai/php-ml) via Composer:
   ```bash
   composer require php-ai/php-ml
3. Upload the index.php file located in the Endpoint/ folder to your PHP-compatible web server.

4. Ensure your server is running and the file is accessible via a public URL (e.g., https://yourdomain.com/index.php).

### 🔗 Connect Unity to Backend
1. Open the Unity project located in the UnityApp/ folder.

2. Navigate to:
'Assets/MyProject/Scripts/Controller/APIManager.cs'

3. Update the apiUrl variable with the public URL of your uploaded index.php file:
'string apiUrl = "https://yourdomain.com/index.php";'

4. Build the Unity project for your desired platform (e.g., Android/iOS).



