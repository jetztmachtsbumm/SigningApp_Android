package de.bytekontrol.signingapp

import android.app.admin.DevicePolicyManager
import android.content.ComponentName
import android.graphics.Color
import android.net.http.SslError
import android.os.Bundle
import android.webkit.SslErrorHandler
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.activity.ComponentActivity

class MainActivity() : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val deviceAdmin = ComponentName(this, SigningAppAdminReceiver::class.java)
        val dpm = getSystemService(DevicePolicyManager::class.java) as DevicePolicyManager

        dpm.setLockTaskPackages(deviceAdmin, arrayOf(packageName))

        if (dpm.isLockTaskPermitted(packageName)) {
            startLockTask()
        }

        val webView = WebView(this)
        webView.setBackgroundColor(Color.WHITE)
        webView.webViewClient = object : WebViewClient() {
            override fun shouldOverrideUrlLoading(view: WebView?, url: String?): Boolean {
                view?.loadUrl(url!!)
                return true
            }

            override fun onReceivedSslError(view: WebView?, handler: SslErrorHandler?, error: SslError?) {
                handler?.proceed()
            }
        }
        val webSettings = webView.settings
        webSettings.javaScriptEnabled = true
        setContentView(webView)

        webView.loadUrl("https://10.0.2.2:4200")
    }
}