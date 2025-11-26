#!/bin/bash


read -p "Enter your GitHub username: " GITHUB_USERNAME
read -p "Enter your GitHub email: " GITHUB_EMAIL
read -p "Enter your GitHub Personal Access Token (PAT): " GITHUB_PAT
read -p "Are you a NEONFORGE maintainer? (y/n): " IS_MAINTAINER && [[ $confirm == [yY] || $confirm == [yY][eE][sS] ]]


if [[ $IS_MAINTAINER == [yY] ]]; then
    NF_MAINTAINER=1
    read -p "Enter your primary NEONFORGE Office 365 email: " NC_USER
fi

echo
echo "Configuring..."
echo

NF_ROOT=`pwd`
NF_REPOS=`dirname ${NF_ROOT}`
NF_TOOLBIN=${NF_ROOT}/ToolBin
NF_BUILD=${NF_ROOT}/Build
NF_CACHE=${NF_ROOT}/Build-cache
NF_SNIPPETS=${NF_ROOT}/Snippets
NF_TEST=${NF_ROOT}/Test
NF_TEMP=/tmp/neonforge

mkdir -p ${NF_TOOLBIN}
mkdir -p ${NF_BUILD}
mkdir -p ${NF_CACHE}
mkdir -p ${NF_SNIPPETS}
mkdir -p ${NF_TEMP}

if [ "$(uname)" == "Darwin" ]; then
    # MacOS platform
    ENV_FILE=~/.zprofile  
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    # GNU/Linux platform
    ENV_FILE=~/.bashrc
fi

touch ${ENV_FILE}

cat <<EOF >> ${ENV_FILE}

# Neon Env
export GITHUB_USERNAME=${GITHUB_USERNAME}
export GITHUB_EMAIL=${GITHUB_EMAIL}
export GITHUB_PAT=${GITHUB_PAT}
export DEV_WORKSTATION=1
export NF_ROOT=${NF_ROOT}
export NF_REPOS=${NF_REPOS}
export NF_TOOLBIN=${NF_TOOLBIN}
export NF_BUILD=${NF_BUILD}
export NF_CACHE=${NF_CACHE}
export NF_SNIPPETS=${NF_SNIPPETS}
export NF_TEST=${NF_TEST}
export NF_TEMP=${NF_TEMP}
export NF_MAINTAINER=${NF_MAINTAINER}
export PATH="${NF_TOOLBIN}:\${PATH}"
EOF

echo "" >> ${ENV_FILE}

echo ${GITHUB_PAT} | docker login ghcr.io -u ${GITHUB_USERNAME} --password-stdin
echo ${GITHUB_PAT} | gh auth login --with-token
git config user.email ${GITHUB_EMAIL}

echo "Finished"
