# 📚 English Learning Application with Spaced Repetition

![Project Logo](src/EnglishApplication.Web/wwwroot/images/logo/leptonxlite/logo-dark.png)

## 🎯 About The Project

English Learning Application is a web-based platform designed to help users learn English vocabulary effectively using the scientifically proven spaced repetition method. The application implements a 6-stage repetition system that optimizes the learning process by presenting words at strategically timed intervals.

## ✨ Key Features

- **🔄 Spaced Repetition System**
  - 6-stage learning process
  - Day 0: Initial learning
  - Day 1: First review
  - Day 3: Second review
  - Day 7: Third review
  - Day 14: Fourth review
  - Day 30: Final review

- **📝 Vocabulary Management**
  - Add custom words with translations
  - Upload images for visual association
  - Add example sentences for context
  - Pre-loaded common word sets

- **🎯 Interactive Quizzes**
  - Adaptive testing system
  - Progress tracking
  - Performance analytics
  - Customizable daily goals

- **📊 Progress Tracking**
  - Detailed statistics
  - Learning progress visualization
  - Downloadable PDF reports
  - Success rate analytics

## 🛠️ Built With

- ASP.NET Core (.NET 9)
- Razor Pages
- ABP Framework
- Entity Framework Core
- JavaScript/jQuery
- Bootstrap 5
- SQL Server

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server
- Node.js and npm

### Installation

1. Clone the repository
2. Navigate to the project directory
3. Install dependencies
4. Update database connection string in `appsettings.json`
5. Apply database migrations
6. Run the application


## 👥 Initial Setup - Creating Student Role

Before using the application, you need to create a "student" role with necessary permissions. Follow these steps:

### 1. Login as Admin
- Use the default admin credentials:
  
### 2. Create Student Role
1. Navigate to `Administration -> Identity Management -> Roles`
2. Click `New Role`
3. Enter role name as "student"
4. Save the role

### 3. Grant Permissions
1. Click on the `Permissions` button next to the "student" role
2. Grant the following permissions:
   - EnglishApplication
     - ✅ Words
       - ✅ Creating new words
       - ✅ Editing words
       - ✅ Deleting words
     - ✅ UserSettings
       - ✅ Creating user settings
       - ✅ Editing user settings
       - ✅ Deleting user settings
     - ✅ WordSamples
       - ✅ Creating word samples
       - ✅ Editing word samples
       - ✅ Deleting word samples
     - ✅ QuizAttempts
       - ✅ Creating quiz attempts
       - ✅ Editing quiz attempts
       - ✅ Deleting quiz attempts
     - ✅ Stats
3. Click `Save` to apply permissions

### 4. Register New Users
- New users registering through the application will automatically be assigned the "student" role
- They will have access to all learning features including:
  - Word management
  - Quiz taking
  - Progress tracking
  - Statistics viewing

> **Note**: Without proper role and permissions setup, users won't be able to access the application's features.




## 📱 Usage

1. **Register/Login**: Create an account or login to start learning

2. **Add Words**: 
   - Add words manually
   - Import pre-defined word sets
   - Add example sentences and images

3. **Take Quizzes**:
   - Complete daily word quizzes
   - Review words based on spaced repetition schedule
   - Track your progress

4. **Monitor Progress**:
   - View statistics
   - Generate progress reports
   - Track learning milestones

## 🎓 Learning Method

The application uses a scientifically proven spaced repetition system with 6 stages:

1. **Initial Learning (Day 0)**
   - First exposure to the word
   - Basic quiz to establish baseline

2. **First Review (Day 1)**
   - Quick revision within 24 hours
   - Reinforces initial learning

3. **Second Review (Day 3)**
   - Review after short interval
   - Strengthens memory retention

4. **Third Review (Day 7)**
   - Weekly checkpoint
   - Tests medium-term retention

5. **Fourth Review (Day 14)**
   - Bi-weekly assessment
   - Confirms learning progress

6. **Final Review (Day 30)**
   - Monthly verification
   - Ensures long-term retention

## 📈 Success Metrics

- Words are considered "learned" after successful completion of all 6 stages
- Progress tracking through comprehensive statistics
- Customizable daily learning goals
- Performance analytics and progress reports

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

[License Type] - See LICENSE file for details

## 📞 Contact

Project Link: [repository-url]

