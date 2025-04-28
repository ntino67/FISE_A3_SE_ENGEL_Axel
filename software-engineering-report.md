# Introduction to Key Software Engineering Tools and Practices

## 1. Versioning Tools and Hosting Platforms

### Git

To introduce you to versioning, there is nothing better than Git. Of course, there are other VCS such as SVN, which is practically the same apart that Git allows you to modify previous commits.

[Git](https://git-scm.com/) is the distributed [version control system](https://git-scm.com/book/en/v2/Getting-Started-About-Version-Control) (VCS).
Nearly every developer in the world uses it to manage their code.
It has quite a monopoly on VCS. Developers use Git to :

* Keep a history of their code changes
* Revert mistakes made in their code
* Collaborate with other developers
* Make backups of their codes
* And much more

To familiarize yourself with Git, RTFM.

More seriously, one of the best part of Git, is that all the documentation is fantastic.

Just run `man git` in your terminal and you'll have access to the entirety of this amazing documentation.

#### Porcelain and Plumbing

In Git, commands are divided into high-level (porcelain) commands and low-level (plumbing) commands. The porcelain commands are the ones that you will use most often as a developer to interact with your code. Some porcelain commands are :

* `git status`
* `git add`
* `git commit`
* `git push`
* `git pull`
* `git log`

Some examples of plumbing commands are:

* `git apply`
* `git commit-tree`
* `git hash-object`

#### Repositoy

The very first step of any project is to create a repository. A git "repository" or "repo" represent a single project.

A repo is essentially just a directory that contins a project. The only difference is that it also contains a hidden `.git` directory. And this hidden directory contains all of its internal tracking and versioning information for the project.

You can do a lot of things but essentially you can see the status of a git repo with `git status`, you can stage changes with `git add`, and commit with `git commit`.

Those 3 commands are the 3 main commands of git.

#### Branching

A git branch allows you to keep track of different changes separately.

Instead of changing the entire project directly, you can create a new branch and work on it. When you're done, if you like the changes, you can merge your branch with the main one. If you don't like the changes, you can simply deletes the changes.

Example:

```
       G - H    lanes_branch
      /
A - B - C - D   main
  \
   E - F        primes_branch
```

We can also have a remote branch is just an external repo with *mostly* the same Git history as our local repo. And now that we now what Git is, let's introduce Github.

### Hosting Platforms

(Github)[https://github.com/] is the most popular website for Git repositories online. That is for hosting "remotes" on a central websites. Github serves several purposes:

* As a backup of all our code in the cloud in case something wrong happens
* As a central place to share our code and collaborate on it with others
* As a public portfolio for our projects

> Git != Github

Git and Github are not the same, git is an open source command line tool for managing code files. Github and its primary competitors, (GitLab)[https://gitlab.com/] and (Bitbucket)[https://bitbucket.org/], are commercial web products that *use* Git.

In Github, such as GitLab and Bitbucket, we can create repo, push our changes and pull changes of others.

### DevOps Implementation

(DevOps)[https://about.gitlab.com/topics/devops/] is the combination of Development (Dev) and Operations (Ops) to increase the efficiency, speed, and security of software development and delivery compared to traditional processes.

The DevOps methodology comprises 4 principles:

1. Automation of the software development cycle
2. Collaboration and communication
3. Continuous improvement and minimization of waste
4. Hyperfocus on user needs with short feedback loops

As you can see Git, Github and their counterparts are deeply tied with DevOps.

These 2 tools together allows us to have:

* Version control
* CI/CD pipelines
* Issue tracking
* Code review

## 2. Project Management Approaches and Tools

In software engineering, project management is essential to organize, plan, and track the progress of development work. It ensures that the project meets its goals on time and within scope.

### Project Management Approaches:
**Agile:**

A flexible methodology that emphasizes iterative progress and frequent client feedback.

Focuses on adaptability to changes during the project.

**Scrum:**

A specific Agile framework that organizes work into sprints (typically 2–4 weeks).

Each sprint delivers a usable increment of the product.

Includes practices like daily stand-ups, backlog grooming, and retrospectives.

**Example:**
In Scrum, a team might plan a 2-week sprint to design a login page and develop an API. They meet every morning for a short update (daily stand-up).

🔵 **Note:**
Other approaches also exist, such as:

**Waterfall**: A sequential, traditional method where each phase must be completed before the next starts.

Kanban: Focuses on continuous delivery and visualizing work, limiting work in progress.

### Project Management Tools:
**Jira:**

A powerful platform mainly used by Agile and Scrum teams.

**Manages:**

Backlogs (task lists)

Sprints (short development cycles)

Tickets (individual work items or issues)

**Trello:**

A simpler, visual tool based on Kanban boards.

Tasks are represented as cards moving across columns like "To Do", "In Progress", and "Done".

**Example:**
In Trello, a "Frontend Tasks" board could include:

"Design landing page" → To Do

"Implement user login" → In Progress

"Fix mobile responsiveness" → Done

🔵 **Note:**
Other popular tools include:

__Asana__: Focused on task tracking and team organization.

__Monday.com__: A visual project management platform combining Kanban, calendars, and timelines.

__ClickUp__: A flexible, all-in-one project management solution.

### Relationship with Our Project:
For our project, we might chose Trello because:

It offers a simple, visual way to manage our tasks.

It helps us clearly assign tasks, monitor progress, and stay organized as a team.

Everyone can easily see who is working on what and what remains to be completed.

## 3. Communication Tools

Effective communication is crucial in software engineering projects, especially within teams. Tools like Microsoft Teams and Discord have become integral in facilitating seamless collaboration, each offering unique features tailored to different team dynamics.

### Microsoft Teams

Microsoft Teams is a comprehensive collaboration platform that integrates chat, video conferencing, file sharing, and app integration, all within the Microsoft 365 ecosystem.

**Key Features:**

- **Persistent Chat and Channels:** Organize conversations by topics, projects, or departments, allowing for structured and searchable communication.

- **Video Conferencing:** Host meetings with features like screen sharing, meeting recordings, live captions, and breakout rooms for subgroup discussions.

- **File Sharing and Collaboration:** Seamless integration with OneDrive and SharePoint enables real-time co-authoring of documents.

- **App Integrations:** Access a wide range of third-party apps and services directly within Teams to enhance productivity.

- **Security and Compliance:** Built-in enterprise-grade security features, including data encryption and compliance with various industry standards.

**Relevance to Our Project:**

For our C# development project, Microsoft Teams offers a centralized hub for communication and collaboration. Its integration with Visual Studio can streamline our development workflow, from code reviews to deployment pipelines.

### Discord

Originally designed for gaming communities, Discord has evolved into a versatile communication platform suitable for various collaborative environments.

**Key Features:**

- **Voice and Video Channels:** Facilitate real-time discussions, stand-up meetings, or pair programming sessions.

- **Text Channels:** Organize conversations by topics, allowing for focused discussions and easy information retrieval.

- **Screen Sharing and Streaming:** Share your screen or stream applications to collaborate effectively during development or debugging sessions.

- **Customization:** Utilize bots and integrations to automate tasks, manage roles, or enhance server functionality.

- **Cross-Platform Availability:** Accessible via desktop, web, and mobile applications, ensuring team connectivity across devices.

**Relevance to Our Project:**

Discord provides a more informal and flexible communication environment, which can be beneficial for quick discussions, brainstorming sessions, or social interactions within the team. Its ease of use and customization options make it a valuable tool for improving team cohesion.

### Comparative Insights

| Feature               | Microsoft Teams                         | Discord                                 |
|-----------------------|-----------------------------------------|-----------------------------------------|
| **Primary Use Case**  | Enterprise collaboration                | Community and team communication        |
| **Integration**       | Deep integration with Microsoft 365     | Supports various third-party integrations |
| **Customization**     | Limited to available apps and settings  | Highly customizable with bots and APIs  |
| **User Interface**    | Professional and structured             | Casual and user-friendly                |
| **Security**          | Enterprise-level security and compliance| Basic security suitable for general use |

### Conclusion

Both Microsoft Teams and Discord offer valuable communication features that can enhance collaboration within our software engineering project. Microsoft Teams is well-suited for structured, formal communication and integrates seamlessly with our development tools. Discord, on the other hand, offers a more relaxed environment conducive to spontaneous discussions and team bonding. Depending on the context, use both platforms could provide a balanced communication strategy.

## 4. Development Environments and Debugging Tools

### Definition
Development Environments (IDEs) are software tools that help developers write, test, and debug code more efficiently. They centralize features like code editing, compiling, and debugging into a single interface, improving productivity and code quality.

### Key Components
* Visual Studio:
Visual Studio is a versatile IDE supporting multiple languages like C#, C++, and Python. It adapts to each language through modular extensions and built-in compilers, making it ideal for complex, cross-technology projects.

* Frameworks:
Frameworks provide a structured foundation for application development:

- React: Builds dynamic web user interfaces.
- Django: Develops secure and rapid web backends.
- Spring Boot: Simplifies Java backend services.

Frameworks speed up development and enforce best practices by offering pre-built components.

* Debugging Tools
Debuggers help identify and fix errors. Key features include:

- Breakpoints: Pause execution to inspect program state.
- Step-by-Step Execution: Follow the program flow.
- Log Analysis: Review runtime information to diagnose issues.

These tools help developers quickly locate bugs and optimize code.

Example
Using Visual Studio, developers can create a backend with ASP.NET Core and a frontend with React, debugging both layers seamlessly within the same environment.

Application to Our Project
- IDE: Visual Studio 2022
- Framework: .NET Core 3.X
- Language: C#

We use Visual Studio’s built-in debugging features to ensure rapid error detection and correction.
Although Node.js was mentioned for rapid prototyping, our project is fully based on .NET technologies and does not require Node.js.

## 5. Other Insights in Software Engineering

Software engineering requires mastering not only development tools but also practices that guarantee scalable and maintainable products. Beyond the technical skills, it demands a strong focus on collaboration, modeling, quality, and agility.

### DevOps Culture

The collaboration between development (Dev) and operations (Ops) teams enhances the speed, quality, and reliability of software delivery. DevOps practices encourage automation (e.g., continuous integration and continuous deployment pipelines) using tools like GitHub Actions, GitLab CI/CD, or Jenkins.  
These practices help ensure that code is tested, validated, and deployed efficiently and consistently.

### UML (Unified Modeling Language)

UML provides a standardized way to model software systems. It uses diagrams such as:
- **Use Case Diagrams** to define user interactions,
- **Class Diagrams** to model the structure of the system,
- **Sequence Diagrams** to show interactions over time.

By offering a visual representation of systems, UML helps improve communication among developers, designers, and stakeholders, ensuring that technical and functional requirements are understood early in the project.

### Software Quality and Testing

Another essential aspect of software engineering is maintaining software quality through rigorous testing practices:
- **Unit Testing** ensures that individual components function correctly.
- **Integration Testing** verifies the interaction between different modules.
- **User Acceptance Testing (UAT)** validates the system against user needs.

Modern frameworks like JUnit, PyTest, or NUnit automate much of the testing process, ensuring reliability and faster feedback loops.

### Conclusion

Software engineering is much more than just writing code.  
It is about designing systems, maintaining high quality, communicating clearly, and continuously improving.  
Mastering these additional skills strengthens both the technical and organizational success of any project.

