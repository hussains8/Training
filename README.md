#!/bin/bash

# Fetch all branches to ensure the local repository is up-to-date
git fetch --all

# Initialize an array to store the branch and author details
declare -a branches

# Get the list of all remote branches
all_branches=$(git branch -r | grep -v "\->")

# Loop through each remote branch
for branch in $all_branches; do
  # Check if the branch is merged into develop
  if git branch -r --merged origin/develop | grep -q "$branch"; then
    # Exclude develop, main, and master branches
    if [[ $branch != "origin/develop" && $branch != "origin/main" && $branch != "origin/master" ]]; then
      clean_branch=$(echo $branch | sed 's|origin/||')
      author=$(git show -s --format='%an' $branch)
      branches+=("$author\t$clean_branch")
    fi
  fi
done

# Sort the branches array by the author names
IFS=$'\n' sorted_branches=($(sort <<<"${branches[*]}"))
unset IFS

# Print the sorted branches
echo "Remote branches merged into 'develop' along with their authors (sorted by author name):"
echo "--------------------------------------------------------------------------------------"
for branch in "${sorted_branches[@]}"; do
  echo -e "$branch"
done
