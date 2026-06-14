pipeline {
    agent any

    environment {
        // Fallback version if automatic detection fails
        DEFAULT_UNITY_VERSION = '6000.0.59f2'
    }

    stages {
        stage('Locate Unity') {
            steps {
                script {
                    // 1. If UNITY_EXE is already defined as a Node/Agent environment variable, use it.
                    if (env.UNITY_EXE) {
                        echo "Using Unity path configured in agent environment variables: ${env.UNITY_EXE}"
                    } else {
                        echo "UNITY_EXE environment variable not defined. Searching dynamically..."
                        
                        // Read project version file on the agent
                        def versionText = readFile 'ProjectSettings/ProjectVersion.txt'
                        def version = env.DEFAULT_UNITY_VERSION
                        
                        def versionLine = versionText.split('\n').find { it.contains('m_EditorVersion:') }
                        if (versionLine) {
                            version = versionLine.split(':')[1].trim()
                        }
                        echo "Target Unity Version: ${version}"

                        // Common paths on Windows and macOS agents
                        def winPath = "C:\\Program Files\\Unity\\Hub\\Editor\\${version}\\Editor\\Unity.exe"
                        def macPath = "/Applications/Unity/Hub/Editor/${version}/Unity.app/Contents/MacOS/Unity"

                        if (fileExists(winPath)) {
                            env.UNITY_EXE = winPath
                        } else if (fileExists(macPath)) {
                            env.UNITY_EXE = macPath
                        } else {
                            error "Could not automatically locate Unity ${version} on this agent. Please define the 'UNITY_EXE' environment variable in your Jenkins Node settings pointing to the executable path."
                        }
                        echo "Located Unity at: ${env.UNITY_EXE}"
                    }
                }
            }
        }

        stage('Prepare Git LFS') {
            steps {
                script {
                    if (isUnix()) {
                        sh 'git lfs pull'
                    } else {
                        bat 'git lfs pull'
                    }
                }
            }
        }

        stage('Build Quest APK') {
            steps {
                script {
                    if (isUnix()) {
                        sh """
                            "${env.UNITY_EXE}" -batchmode -quit -projectPath "\$WORKSPACE" -executeMethod BuildFlavors.BuildAndroid64 -logFile build.log
                        """
                    } else {
                        bat """
                            "${env.UNITY_EXE}" -batchmode -quit -projectPath "%WORKSPACE%" -executeMethod BuildFlavors.BuildAndroid64 -logFile build.log
                        """
                    }
                }
            }
        }
    }

    post {
        always {
            // Archive the Unity build log for troubleshooting
            archiveArtifacts artifacts: 'build.log', allowEmptyArchive: true
        }
        success {
            // Archive the built Quest APK on a successful build
            archiveArtifacts artifacts: 'builds/TheWorldBeyond_64.apk', fingerprint: true
        }
    }
}
