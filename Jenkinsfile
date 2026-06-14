pipeline {
    agent { label 'Windows' }

    environment {
        UNITY_EXE = 'C:\\Program Files\\Unity\\Hub\\Editor\\6000.4.11f1\\Editor\\Unity.exe'
    }

    stages {
        stage('Prepare Git LFS') {
            steps {
                bat 'git lfs pull'
            }
        }

        stage('Build Quest APK') {
            steps {
                bat """
                    "${env.UNITY_EXE}" -batchmode -quit -projectPath "%WORKSPACE%" -executeMethod BuildFlavors.BuildAndroid64 -logFile build.log
                """
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'build.log', allowEmptyArchive: true
        }
        success {
            archiveArtifacts artifacts: 'builds/TheWorldBeyond_64.apk', fingerprint: true
        }
    }
}
