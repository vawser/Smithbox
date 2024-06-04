// Automatically Generated

namespace HKLib.hk2018;

public class hkbAttachmentModifier : hkbModifier, hkbVerifiable
{
    public hkbEventProperty m_sendToAttacherOnAttach = new();

    public hkbEventProperty m_sendToAttacheeOnAttach = new();

    public hkbEventProperty m_sendToAttacherOnDetach = new();

    public hkbEventProperty m_sendToAttacheeOnDetach = new();

    public hkbAttachmentSetup? m_attachmentSetup;

    public hkbHandle? m_attacherHandle;

    public hkbHandle? m_attacheeHandle;

    public int m_attacheeLayer;

}

