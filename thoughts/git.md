# Git

Fork client is great but has a small one time price: <https://git-fork.com/>

Here are some commands I often use:

    --Take last commit off and stage the changes instead
    git reset --soft HEAD~1

    --Stage all changes
    git add .
    
    --Commit with message
    git commit -m “Some message here”
    
    --Commit with message in editor
    git commit
    
    --Amend
    git commit –-amend
    
    ---Amend with message. Above version without message pulls earlier commit message into  editor so it’s easy to amend.
    git commit –-amend -m “Some message here”
    
    --Status
    git status
    
    --Create branch and check it out
    git checkout -b SomeNewBranchName
    
    --Check out existing branch
    git checkout SomeExistingBranchName
    
    --Delete local branch. Use -D to force.
    git branch -d SomeLocalBranchName
    
    --Squash merge into current branch from another branch. This takes all commits on   OtherBranch and merges into current but without committing changes. Use git commit after  to be able to edit the squashed messages. Also note that it deletes author information.
    git merge –-squash OtherBranch
    
    --Git stash:
    git stash -m “SomeName”
    
    --List stashes
    git stash list
    
    --Apply stash to working copy
    git stash apply StashNumberShownInList
    
    --To just apply latest stash and remove it from the stash list do
    git stash pop
    
    --To remove a single entry
    git stash drop StashNumberShownInList
    
    --To clear the stash
    git stash clear
    
    --Push given you are in the branch you want to push. To push to another branch: git push    <remote> <local_branch>:<remote_name>
    git push origin NameOfYourBranchHasToMatchRemote
    
    --Pull – works remotely. Remember rebase if wanted
    git pull -–rebase
    
    --Pull rebase from master instead of current branch as an alternative to pull squash:
    git pull --rebase origin master
    
    --Show local branches
    git branch

    --Remove local commits by resetting to head @ origin
    git reset --hard origin/CurrentBranch
    
    --Rarely used:
    git remote show origin
    git remote prune origin
    
    --git clean -n shows what would be deleted by Git clean. change to -f to actually run with force. Alternatively use `git stash push -a` to ensure a backup if something bad happens.
    git clean -Xdn
        -Careful -x removes EVERYTHING, -X only removes ignored files

    --git log has 100's of arguments for fine tuning. See this for some examples also: <https://stackoverflow.com/questions/1441010/the-shortest-possible-output-from-git-log-containing-author-and-date>
    --also ;q to exit viewing logs
    --and end with >log.txt to output to file.
    --date=local --after="2014-02-12T16:36:00-07:00" can be used or alternatively just --since="2020-01-01"
    git log --author="Asbjørn"
    git log --pretty=format:"%h %C(auto,yellow)%ad%C(auto,green)%x09%<(50,trunc)%s%C(auto,reset)" --date=short --author="Asbjørn" --shortstat
    git log --pretty=format:"%h %ad%x09%<(50,trunc)%s" --date=short --author="Asbjørn"

    --Add a first empty commit to a new repo:
    `git commit --allow-empty -m "Initial empty commit"`

    --To interactively rebase/edit first commit in a repo use:
    `git rebase --root`

    --To reset author from a given commit and forward use. DO NOTE THIS UPDATES DATES AS WELL. Also remember distinction between author and committer:
    `git rebase -r <someCommitHash> --exec 'git commit --amend --no-edit --reset-author'`

    --To find strings in historic commits use pickaxe (git log -S) like:
    `git log -S'using MyLibrary.Logging.LoggingServices;' --oneline`

    --Not git but cmd in windows - to terminate END on long outputs press ;q
