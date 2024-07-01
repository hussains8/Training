#!/bin/bash

# Fetch all branches to ensure the local repository is up-to-date
git fetch --all

# Ensure we are on the develop branch
git checkout develop

# Initialize an array to store the branch and author details
declare -a branches

# Get the list of all remote branches
all_branches=$(git branch -r | grep -v "\->")

# Loop through each remote branch
for branch in $all_branches; do
  # Check if the branch is merged into develop
  if git merge-base --is-ancestor $branch origin/develop; then
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

# Print and save the sorted branches to a file
output_file="merged_branches_in_develop.txt"
echo "Remote branches merged into 'develop' along with their authors (sorted by author name):" > $output_file
echo "--------------------------------------------------------------------------------------" >> $output_file
for branch in "${sorted_branches[@]}"; do
  echo -e "$branch" >> $output_file
done

echo "Results saved in $output_file"
