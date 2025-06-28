using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// A class used to display an item's details in GUI
	/// </summary>
	public class InventoryDetails : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
        /// �ο��ֿ⣨������Դ��
        [MMInformation("ָ��Ҫ�ڴ������������ʾ��Ʒ����Ĳֿ����ơ�����'ȫ��'����ʾ������Ʒ���飨���������ֿ⣩��", MMInformationAttribute.InformationType.Info, false)]
        [MMLabel("Ŀ��ֿ�")]
        public string TargetInventoryName;
        [MMLabel("���ID")]
        public string PlayerID = "Player1";
        /// �Ƿ�ȫ����ʾ���вֿ���Ʒ
        [MMLabel("ȫ��ģʽ")]
        [Tooltip("���ú���ʾ������Ʒ���飨����Ŀ��ֿ⣩")]
        public bool Global = false;

        /// ������嵱ǰ�Ƿ�����
        public virtual bool Hidden { get; protected set; }

        [Header("Ĭ��ֵ")]
        [MMInformation("����'�ղ�����'��ѡ��ղ��ʱ������ʾ������塣", MMInformationAttribute.InformationType.Info, false)]

        /// ѡ��ղ��ʱ�Ƿ��������
        [MMLabel("�ղ�����")]
        [Tooltip("ѡ��ղ��ʱ�Զ������������")]
        public bool HideOnEmptySlot = true;

        [MMInformation("��������Ʒѡ��ʱ��������Ĭ����ʾֵ��δ���ÿղ�����ʱ��Ч����", MMInformationAttribute.InformationType.Info, false)]

        /// Ĭ�ϱ���
        [MMLabel("Ĭ�ϱ���")]
        [Tooltip("����Ʒѡ��ʱ��ʾ�ı����ı�")]
        public string DefaultTitle;

        /// Ĭ�ϼ������
        [MMLabel("Ĭ�ϼ���")]
        [Tooltip("����Ʒѡ��ʱ��ʾ�ļ������")]
        public string DefaultShortDescription;

        /// Ĭ������
        [MMLabel("Ĭ������")]
        [Tooltip("����Ʒѡ��ʱ��ʾ����������")]
        public string DefaultDescription;

        /// Ĭ������
        [MMLabel("Ĭ������")]
        [Tooltip("����Ʒѡ��ʱ��ʾ�������ı�")]
        public string DefaultQuantity;

        /// Ĭ��ͼ��
        [MMLabel("Ĭ��ͼ��")]
        [Tooltip("����Ʒѡ��ʱ��ʾ��ͼ��")]
        public Sprite DefaultIcon;

        [Header("��Ϊ����")]
        [MMInformation("���������������Ϸ����ʱ��Ĭ�Ͽɼ�״̬��", MMInformationAttribute.InformationType.Info, false)]

        /// ����ʱ�Ƿ��������
        [MMLabel("��������")]
        [Tooltip("��Ϸ����ʱ�Զ������������")]
        public bool HideOnStart = true;

        [Header("�����")]
        [MMInformation("����������UI�����", MMInformationAttribute.InformationType.Info, false)]

        /// ͼ������
        [MMLabel("ͼ�����")]
        [Tooltip("��ʾ��Ʒͼ���Image���")]
        public Image Icon;

        /// the title container object
        [MMLabel("�������")]
        [Tooltip("��ʾ��Ʒ�����TMP Text���")]
        public TMPro.TMP_Text Title;

        /// the short description container object
        [MMLabel("�������")]
        [Tooltip("��ʾ��Ʒ���������TMP Text���")]
        public TMPro.TMP_Text ShortDescription;

        /// the description container object
        [MMLabel("�������")]
        [Tooltip("��ʾ��Ʒ����������TMPText���")]
        public TMPro.TMP_Text Description;

        /// the quantity container object
        [MMLabel("�������")]
        [Tooltip("��ʾ��Ʒ������TMPText���")]
        public TMPro.TMP_Text Quantity;

		protected float _fadeDelay=0.2f;
		protected CanvasGroup _canvasGroup;

		/// <summary>
		/// On Start, we grab and store the canvas group and determine our current Hidden status
		/// </summary>
		protected virtual void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();

			if (HideOnStart)
			{
				_canvasGroup.alpha = 0;
			}

			if (_canvasGroup.alpha == 0)
			{
				Hidden = true;
			}
			else
			{
				Hidden = false;
			}
		}

        /// <summary>
        /// Starts the display coroutine or the panel's fade depending on whether or not the current slot is empty
        /// ���ݵ�ǰ����Ƿ�Ϊ�գ�������ʾЭ�̻����ĵ��뵭��Ч��
        /// </summary>
        /// <param name="item">Item.</param>
        public virtual void DisplayDetails(InventoryItem item)
		{
		

			if (InventoryItem.IsNull(item))
			{
				if (HideOnEmptySlot && !Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
					Hidden=true;
				}
				if (!HideOnEmptySlot)
				{
					StartCoroutine(FillDetailFieldsWithDefaults(0));
				}
			}
			else
			{
				StartCoroutine(FillDetailFields(item,0f));

				if (HideOnEmptySlot && Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,1f));
					Hidden=false;
				}
			}

		}

		/// <summary>
		/// Fills the various detail fields with the item's metadata
		/// </summary>
		/// <returns>The detail fields.</returns>
		/// <param name="item">Item.</param>
		/// <param name="initialDelay">Initial delay.</param>
		protected virtual IEnumerator FillDetailFields(InventoryItem item, float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = item.ItemName ; }
			if (ShortDescription!=null) { ShortDescription.text = item.ShortDescription;}
			if (Description!=null) { Description.text = item.Description;}
			if (Quantity!=null) { Quantity.text = item.Quantity.ToString();}
			if (Icon!=null) { Icon.sprite = item.Icon;}
			
			if (HideOnEmptySlot && !Hidden && (item.Quantity == 0))
			{
				StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
				Hidden=true;
			}
		}

		/// <summary>
		/// Fills the detail fields with default values.
		/// </summary>
		/// <returns>The detail fields with defaults.</returns>
		/// <param name="initialDelay">Initial delay.</param>
		protected virtual IEnumerator FillDetailFieldsWithDefaults(float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = DefaultTitle ;}
			if (ShortDescription!=null) { ShortDescription.text = DefaultShortDescription;}
			if (Description!=null) { Description.text = DefaultDescription;}
			if (Quantity!=null) { Quantity.text = DefaultQuantity;}
			if (Icon!=null) { Icon.sprite = DefaultIcon;}
		}

		/// <summary>
		/// Catches MMInventoryEvents and displays details if needed
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (!Global && (inventoryEvent.TargetInventoryName != this.TargetInventoryName))
			{
				return;
			}

			if (inventoryEvent.PlayerID != PlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
			
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.UseRequest:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.InventoryOpens:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.Drop:
					DisplayDetails (null);
					break;
				case MMInventoryEventType.EquipRequest:
					DisplayDetails (null);
					break;
			}
		}

		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}