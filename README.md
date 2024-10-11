# Student-Teacher Question & Answer Platform

## Overview

The **Student-Teacher Question & Answer Platform** is a web application designed to facilitate communication between students and teachers. Students can ask questions, view responses from teachers, and track their queries, while teachers can answer questions and manage their responses. This project is built on ASP.NET Core using a three-layer architecture and MSSQL Database.

## Features

- **User Registration and Authentication**
  - Users can register with their email address and password.
  - Upon registration as a student, users must provide their Name, Institute Name, and Institute ID Card Number.
  - Users can log in and log out of their accounts.

- **Student Dashboard**
  - Students see the most recent questions asked by all students.
  - By clicking on a question, students can view the question details and any existing answers.
  - Students have a separate tab (Activity) to view their questions and responses from teachers.

- **Teacher Dashboard**
  - Teachers can view the latest questions asked by students.
  - Teachers can select a question to answer from the pool of recent questions.
  - Teachers have a separate tab (Activity) to view questions they have responded to.

- **Question Management**
  - Teachers can reply to any question.
  - Students can reply only to their own questions.
  - The questioner can delete their question unless it has received a response from a teacher.

- **Security**
  - User authentication ensures that only authenticated users can access specific parts of the website.

## Project Structure

- **Presentation Layer**: `test_dotnet1`
- **Business Logic Layer**: `test_dotnet1_Models`
- **Data Access Layer**: `test_dotnet_Data_Access`

## Models

- **ApplicationUser**: Represents the user of the application (students and teachers).
- **Question**: Represents a question asked by a student.
- **Answer**: Represents a response provided by a teacher.

## Technologies Used

- ASP.NET Core Version 6
- Entity Framework Core
- MSSQL Server (or LocalDB)
- jQuery for frontend
- HTML/CSS for frontend


## Setup Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/afzalyen/StudentTeacher_QA_Platform_WEB.git
   cd StudentTeacher_QA_Platform_WEB


## Super Admin Credentials

- **Email**: `superadmin@example.com`  
- **Password**: `SuperAdmin@123`









