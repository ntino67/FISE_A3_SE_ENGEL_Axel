# Introduction to Key Software Engineering Tools and Practices

## 1. Versioning Tools and Hosting Platforms

|> Definition: Tools like Git and SVN (Subversion) are version control systems that manage changes to source code over time, allowing multiple developers to collaborate effectively.

### Git

To introduce you to versioning, there is nothing better than Git. 

(Git)[https://git-scm.com/] is the distributed (version control system)[https://git-scm.com/book/en/v2/Getting-Started-About-Version-Control] (VCS).
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

Some examples of plumbig commands are:

* `git apply`
* `git commit-tree`
* `git hash-object`

Hosting Platforms:

GitHub, GitLab, and Bitbucket host repositories, support DevOps practices (CI/CD pipelines, issue tracking, code review), and help in automating deployments.

Relationship with Project:
We use Git and GitHub to manage our project’s codebase, track changes, and collaborate as a team while ensuring code integrity.

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

Definition: Tools that facilitate real-time collaboration and discussions among team members, regardless of location.

Examples:

Microsoft Teams: Integrates messaging, meetings, and file sharing.

Discord: Offers voice channels, chat, and screen sharing, especially popular with developer communities.

Relationship with Project:
We use Teams/Discord for daily communication, quick troubleshooting, and efficient coordination between developers.

## 4. Development Environments and Debugging Tools

Definition: Development Environments (IDEs) are tools that help programmers write, test, and debug their code more easily.

Examples:

Visual Studio: A powerful IDE for C#, C++, Python, and more.

Frameworks: React, Django, Spring Boot depending on the application.

Debugging Tools: Built-in debuggers, breakpoints, log analysis.

Relationship with Project:
We use Visual Studio Code and Node.js framework for rapid development. Debugging tools ensure quick identification and resolution of errors.

## 5. Other Insights: DevOps Integration
DevOps Culture:
The collaboration between development and operations enhances software delivery speed and reliability. Tools like GitHub Actions or GitLab CI/CD automate testing and deployment pipelines.

Personal Feedback:
Working with Git and GitHub significantly improved our team's organization and transparency. Setting up Trello boards gave clarity on individual responsibilities.

Conclusion
Understanding and mastering these tools and approaches is fundamental for efficient, professional software development. They strengthen project structure, team collaboration, and overall product quality — essentials for any successful software engineering endeavor.

Important Reminders for Submission
Save this document as a PDF or a Markdown/Word file.

Push it to your GitHub repository inside a dedicated branch before 4 PM today.
